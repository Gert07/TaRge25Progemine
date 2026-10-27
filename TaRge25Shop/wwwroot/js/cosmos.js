(() => {
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const random = (min, max) => min + Math.random() * (max - min);
    const clamp = (value, min, max) => Math.max(min, Math.min(max, value));
    const entities = [];
    const bolts = [];
    const explosions = [];
    let suspended = false;
    let frame = 0;
    let previousTime = 0;

    document.querySelectorAll('[data-starfield]').forEach(field => {
        const stars = document.createDocumentFragment();
        const count = innerWidth < 600 ? 65 : 115;
        for (let index = 0; index < count; index++) {
            const star = document.createElement('span');
            star.className = 'space-star';
            star.style.left = `${random(0, 100)}%`;
            star.style.top = `${random(0, 100)}%`;
            star.style.setProperty('--star-size', `${random(.8, 2.2)}px`);
            star.style.setProperty('--star-alpha', random(.25, .85).toFixed(2));
            star.style.setProperty('--star-time', `${random(12, 36)}s`);
            star.style.setProperty('--star-delay', `${random(-35, 0)}s`);
            star.style.setProperty('--star-dx', `${random(-25, 25)}px`);
            star.style.setProperty('--star-dy', `${random(-20, 20)}px`);
            stars.append(star);
        }
        field.append(stars);
    });

    function chooseDirection(item) {
        const direction = random(0, Math.PI * 2);
        const speed = item.kind === 'world' ? random(15, innerWidth < 600 ? 28 : 44) : item.kind === 'mascot' ? random(3, 8) : random(5, 16);
        item.tx = Math.cos(direction) * speed;
        item.ty = Math.sin(direction) * speed;
        item.turnIn = random(.8, 3.2);
        item.spinTarget = random(-1, 1) * (item.kind === 'planet' ? 2 : item.kind === 'mascot' ? 4 : 7);
    }

    function measure(item, initial = false) {
        const width = item.parent.clientWidth;
        const height = item.parent.clientHeight;
        const oldWidth = item.parentWidth || width;
        const oldHeight = item.parentHeight || height;
        item.width = item.element.offsetWidth;
        item.height = item.element.offsetHeight;
        item.parentWidth = width;
        item.parentHeight = height;
        if (item.kind === 'raider') {
            positionRaider(item);
            return;
        }
        if (item.kind === 'ship') {
            // Hover around the chosen positions, including after viewport changes.
            const insetX = Math.min(width / 2, item.width / 2 + 14);
            const insetY = Math.min(height / 2, item.height / 2 + 14);
            item.anchorX = clamp(width * Number(item.element.dataset.x) / 100, insetX, width - insetX);
            item.anchorY = clamp(height * Number(item.element.dataset.y) / 100, insetY, height - insetY);
            hoverShip(item, 0);
            return;
        }
        if (item.kind === 'world') {
            const header = item.parent.querySelector('.orbit-header').offsetHeight;
            const footer = item.parent.querySelector('.orbit-footer').offsetHeight;
            item.minX = item.width / 2 + 14;
            item.maxX = Math.max(item.minX, width - item.width / 2 - 14);
            item.minY = header + item.height / 2 + 10;
            item.maxY = Math.max(item.minY, height - footer - item.height / 2 - 10);
        } else if (item.kind === 'mascot') {
            item.minX = width * .4;
            item.maxX = width * .6;
            item.minY = height * .4;
            item.maxY = height * .6;
        } else {
            item.minX = -width * .12;
            item.maxX = width * 1.12;
            item.minY = -height * .12;
            item.maxY = height * 1.12;
        }
        if (initial) {
            let x = Number(item.element.dataset.x || 50) / 100;
            let y = Number(item.element.dataset.y || 50) / 100;
            const jitter = item.kind === 'mascot' ? 0 : .025;
            item.x = width * (x + random(-jitter, jitter));
            item.y = height * (y + random(-jitter, jitter));
            if (item.kind === 'world') {
                // Random starting coordinates, with room around previously placed links.
                const peers = entities.filter(other => other.kind === 'world');
                let best = { score: -1, x: item.x, y: item.y };
                for (let attempt = 0; attempt < 48; attempt++) {
                    const candidate = { x: random(item.minX, item.maxX), y: random(item.minY, item.maxY) };
                    const score = peers.length ? Math.min(...peers.map(other => Math.hypot(
                        (candidate.x - other.x) / ((item.width + other.width) * .43 + 14),
                        (candidate.y - other.y) / ((item.height + other.height) * .43 + 10)
                    ))) : 2;
                    if (score > best.score) best = { ...candidate, score };
                    if (score > 1.12) break;
                }
                item.x = best.x;
                item.y = best.y;
            }
        } else {
            item.x *= width / oldWidth;
            item.y *= height / oldHeight;
        }
        item.x = clamp(item.x, item.minX, item.maxX);
        item.y = clamp(item.y, item.minY, item.maxY);
    }

    function hoverShip(item, dt) {
        const destroyer = item.element.dataset.ship === 'destroyer';
        item.hoverTime += dt * (destroyer ? .38 : .55);
        const phase = item.hoverTime;
        const motion = reducedMotion.matches ? 0 : 1;
        item.x = item.anchorX + Math.sin(phase) * (destroyer ? 8 : 10) * motion;
        item.y = item.anchorY + Math.sin(phase * .8) * (destroyer ? 6 : 8) * motion;
        item.rotation = Math.sin(phase * .6) * .8 * motion;
    }

    function raiderPositionAt(item, progress) {
        // Use the visible viewport even when the homepage is taller on a phone.
        const bandHeight = Math.min(item.parentHeight, window.innerHeight || item.parentHeight) / 3;
        const inset = Math.min(bandHeight / 2, item.height / 2 + item.width * .06 + 8);
        const laneHeight = Math.max(0, bandHeight - inset * 2);
        const flight = item.flight;
        if (!flight) {
            return { x: item.parentWidth + item.width, y: bandHeight / 2, rotation: 0 };
        }
        const phase = progress * Math.PI * 2;
        const lane = flight.startY + (flight.endY - flight.startY) * progress;
        return {
            x: item.parentWidth + item.width - progress * (item.parentWidth + item.width * 2),
            y: clamp(inset + laneHeight * lane + Math.sin(phase) * 6, inset, bandHeight - inset),
            rotation: Math.sin(phase) * 6
        };
    }

    function positionRaider(item) {
        Object.assign(item, raiderPositionAt(item, item.flight?.progress ?? 0));
    }

    function updateRaider(item, dt) {
        if (!item.flight) {
            item.waitIn -= dt;
            if (item.waitIn > 0) return;
            const startY = random(.15, .85);
            item.flight = { progress: 0, duration: random(6, 9), startY, endY: clamp(startY + random(-.15, .15), .1, .9) };
            item.fireIn = .2;
            item.element.classList.add('is-flying');
        }
        item.flight.progress += dt / item.flight.duration;
        positionRaider(item);
        if (item.flight.progress >= 1) {
            item.flight = null;
            item.waitIn = random(8, 18);
            item.element.classList.remove('is-flying');
            return;
        }
        const target = entities.find(other => other.parent === item.parent && other.element.dataset.ship === 'destroyer');
        item.fireIn -= dt;
        // Attack while approaching from the right; stop once the target is behind us.
        if (target && item.fireIn <= 0 && item.x < item.parentWidth - item.width * .2 && item.x > target.x + target.width) {
            fireAttack(item, target);
            item.fireIn = random(.45, .85);
        }
    }

    function hullPoint(item, offset) {
        const angle = item.rotation * Math.PI / 180;
        const x = offset.x * item.width;
        const y = offset.y * item.height;
        return { x: item.x + x * Math.cos(angle) - y * Math.sin(angle), y: item.y + x * Math.sin(angle) + y * Math.cos(angle) };
    }

    function fireAttack(item, target) {
        const hitChance = clamp(Number(item.element.dataset.hitChance ?? .4), 0, 1);
        const hit = Math.random() < hitChance;
        // These offsets sit on the solid central hull, away from transparent margins.
        const offset = { x: random(.06, .2), y: random(.06, .2) };
        const aim = hit ? hullPoint(target, offset) : { x: target.x + target.width * .7, y: target.y - target.height * .9 };
        const muzzle = hullPoint(item, { x: -.46, y: .09 });
        const angle = Math.atan2(aim.y - muzzle.y, aim.x - muzzle.x);
        const element = document.createElement('span');
        element.className = 'laser-bolt laser-attack';
        const speed = 720;
        const bolt = {
            element, ...muzzle,
            vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed,
            angle: angle * 180 / Math.PI,
            life: Math.hypot(aim.x - muzzle.x, aim.y - muzzle.y) / speed + .6,
            speed, target: hit ? target : null, offset
        };
        paintBolt(bolt);
        item.parent.append(element);
        bolts.push(bolt);
    }

    function explode(target, offset) {
        const element = document.createElement('div');
        element.className = 'hull-explosion';
        const core = document.createElement('span');
        core.className = 'explosion-core';
        const ring = document.createElement('span');
        ring.className = 'explosion-ring';
        element.append(core, ring);
        const sparks = Array.from({ length: 8 }, (_, index) => {
            const spark = document.createElement('span');
            spark.className = 'explosion-spark';
            element.append(spark);
            return { element: spark, angle: index * Math.PI / 4 + random(-.15, .15), speed: random(35, 75) };
        });
        const explosion = { element, core, ring, sparks, target, offset, age: 0 };
        paintExplosion(explosion);
        target.parent.append(element);
        explosions.push(explosion);
    }

    function paintExplosion(explosion) {
        const progress = explosion.age / .8;
        const point = hullPoint(explosion.target, explosion.offset);
        explosion.element.style.transform = `translate3d(${point.x.toFixed(2)}px, ${point.y.toFixed(2)}px, 0)`;
        explosion.element.style.opacity = (1 - progress).toFixed(2);
        explosion.core.style.transform = `scale(${(.3 + progress * 1.5).toFixed(2)})`;
        explosion.core.style.opacity = (1 - progress).toFixed(2);
        explosion.ring.style.transform = `scale(${(.2 + progress * 2.2).toFixed(2)})`;
        explosion.sparks.forEach(spark => {
            const distance = spark.speed * explosion.age;
            spark.element.style.transform = `translate(${(Math.cos(spark.angle) * distance).toFixed(2)}px, ${(Math.sin(spark.angle) * distance).toFixed(2)}px) rotate(${(spark.angle * 180 / Math.PI).toFixed(2)}deg)`;
        });
    }

    function updateExplosions(dt) {
        for (let index = explosions.length - 1; index >= 0; index--) {
            const explosion = explosions[index];
            explosion.age += dt;
            if (explosion.age >= .8) {
                explosion.element.remove();
                explosions.splice(index, 1);
            } else paintExplosion(explosion);
        }
    }

    function clearEffects() {
        bolts.forEach(bolt => bolt.element.remove());
        explosions.forEach(explosion => explosion.element.remove());
        bolts.length = 0;
        explosions.length = 0;
    }

    function paint(item) {
        const rotation = item.kind === 'world' ? 0 : item.rotation;
        item.element.style.transform = `translate3d(${item.x.toFixed(2)}px, ${item.y.toFixed(2)}px, 0) translate(-50%, -50%) rotate(${rotation.toFixed(2)}deg)`;
        if (item.kind === 'world') item.element.style.setProperty('--art-rotation', `${item.rotation.toFixed(2)}deg`);
    }

    document.querySelectorAll('[data-drift]').forEach(element => {
        const kind = element.dataset.drift;
        const parent = kind === 'world' ? element.closest('.playground') : element.parentElement;
        const item = { element, parent, kind, held: false, rotation: random(-12, 12), spin: 0 };
        if (kind === 'raider') {
            item.rotation = 0;
            item.flight = null;
            item.waitIn = random(3, 6);
        } else if (kind === 'ship') {
            item.rotation = 0;
            item.hoverTime = random(0, Math.PI * 2);
            item.fireIn = element.dataset.ship === 'destroyer' ? 1.4 : random(.3, 1.2);
            item.salvo = 0;
        } else {
            chooseDirection(item);
            item.vx = item.tx;
            item.vy = item.ty;
        }
        measure(item, true);
        paint(item);
        element.classList.add('drift-ready');
        if (kind === 'world') {
            const hold = () => { item.held = true; };
            const release = () => { item.held = element.matches(':hover') || element.contains(document.activeElement); };
            element.addEventListener('pointerenter', hold);
            element.addEventListener('pointerleave', release);
            element.addEventListener('focusin', hold);
            element.addEventListener('focusout', () => queueMicrotask(release));
        }
        entities.push(item);
    });

    function bounce(item) {
        if (item.kind === 'ship' || item.kind === 'raider') return;
        if (item.x < item.minX || item.x > item.maxX) {
            item.x = clamp(item.x, item.minX, item.maxX);
            const direction = item.x === item.minX ? 1 : -1;
            item.vx = Math.abs(item.vx) * direction;
            item.tx = Math.abs(item.tx) * direction;
            item.turnIn = Math.min(item.turnIn, .6);
        }
        if (item.y < item.minY || item.y > item.maxY) {
            item.y = clamp(item.y, item.minY, item.maxY);
            const direction = item.y === item.minY ? 1 : -1;
            item.vy = Math.abs(item.vy) * direction;
            item.ty = Math.abs(item.ty) * direction;
            item.turnIn = Math.min(item.turnIn, .6);
        }
    }

    function fireLasers(item) {
        const targets = entities.filter(other => other.parent === item.parent && other.kind === 'raider' && other.flight && other.x > 0 && other.x < item.parentWidth);
        if (!targets.length) return false;
        const target = targets.reduce((nearest, candidate) =>
            Math.hypot(candidate.x - item.x, candidate.y - item.y) < Math.hypot(nearest.x - item.x, nearest.y - item.y) ? candidate : nearest);
        const speed = 590;
        // Lead the moving fighter instead of firing along a fixed decorative angle.
        [{ x: .16, y: .15 }, { x: .32, y: .23 }].forEach(offset => {
            const muzzle = hullPoint(item, offset);
            let aim = { x: target.x, y: target.y };
            for (let iteration = 0; iteration < 3; iteration++) {
                const travelTime = Math.hypot(aim.x - muzzle.x, aim.y - muzzle.y) / speed;
                aim = raiderPositionAt(target, Math.min(1, target.flight.progress + travelTime / target.flight.duration));
            }
            const angle = Math.atan2(aim.y - muzzle.y, aim.x - muzzle.x);
            const element = document.createElement('span');
            element.className = 'laser-bolt laser-imperial';
            const bolt = {
                element, ...muzzle,
                vx: Math.cos(angle) * speed,
                vy: Math.sin(angle) * speed,
                angle: angle * 180 / Math.PI,
                life: Math.hypot(aim.x - muzzle.x, aim.y - muzzle.y) / speed + .25
            };
            paintBolt(bolt);
            item.parent.append(element);
            bolts.push(bolt);
        });
        return true;
    }

    function paintBolt(bolt) {
        bolt.element.style.transform = `translate3d(${bolt.x.toFixed(2)}px, ${bolt.y.toFixed(2)}px, 0) rotate(${bolt.angle.toFixed(2)}deg)`;
        bolt.element.style.opacity = Math.min(1, bolt.life * 4).toFixed(2);
    }

    function updateLasers(dt) {
        for (let index = bolts.length - 1; index >= 0; index--) {
            const bolt = bolts[index];
            if (bolt.target) {
                const impact = hullPoint(bolt.target, bolt.offset);
                const distance = Math.hypot(impact.x - bolt.x, impact.y - bolt.y);
                const angle = Math.atan2(impact.y - bolt.y, impact.x - bolt.x);
                if (distance <= bolt.speed * dt) {
                    explode(bolt.target, bolt.offset);
                    bolt.element.remove();
                    bolts.splice(index, 1);
                    continue;
                }
                // Track the small hover offset so hits stay attached to the visible hull.
                bolt.vx = Math.cos(angle) * bolt.speed;
                bolt.vy = Math.sin(angle) * bolt.speed;
                bolt.angle = angle * 180 / Math.PI;
            }
            bolt.life -= dt;
            if (bolt.life <= 0) {
                bolt.element.remove();
                bolts.splice(index, 1);
                continue;
            }
            bolt.x += bolt.vx * dt;
            bolt.y += bolt.vy * dt;
            paintBolt(bolt);
        }
    }

    function separateWorlds(dt) {
        const worlds = entities.filter(item => item.kind === 'world');
        for (let a = 0; a < worlds.length; a++) {
            for (let b = a + 1; b < worlds.length; b++) {
                const first = worlds[a];
                const second = worlds[b];
                const sx = (first.width + second.width) * .43 + 14;
                const sy = (first.height + second.height) * .43 + 10;
                let nx = (second.x - first.x) / sx;
                let ny = (second.y - first.y) / sy;
                let distance = Math.hypot(nx, ny);
                if (distance >= 1) continue;
                if (distance < .001) { nx = random(-1, 1); ny = random(-1, 1); distance = Math.hypot(nx, ny) || 1; }
                const force = 1 - distance;
                const pushX = nx / distance * sx * force * .18;
                const pushY = ny / distance * sy * force * .18;
                if (!first.held) {
                    first.x -= pushX;
                    first.y -= pushY;
                    first.vx -= nx / distance * force * 65 * dt;
                    first.vy -= ny / distance * force * 65 * dt;
                }
                if (!second.held) {
                    second.x += pushX;
                    second.y += pushY;
                    second.vx += nx / distance * force * 65 * dt;
                    second.vy += ny / distance * force * 65 * dt;
                }
            }
        }
    }

    const canRun = () => !suspended && !document.hidden && !reducedMotion.matches;
    function tick(time) {
        frame = 0;
        if (!canRun()) { previousTime = 0; return; }
        const dt = previousTime ? Math.min((time - previousTime) / 1000, .05) : 0;
        previousTime = time;
        const menuOpen = !!document.querySelector('.world-menu[open]');
        entities.forEach(item => {
            if (item.kind === 'world' && (item.held || menuOpen)) return;
            if (item.kind === 'raider') {
                updateRaider(item, dt);
                return;
            }
            if (item.kind === 'ship') {
                hoverShip(item, dt);
                item.fireIn -= dt;
                if (item.fireIn <= 0 && fireLasers(item)) {
                    item.salvo = (item.salvo + 1) % 3;
                    item.fireIn = item.salvo ? .22 : random(2.4, 4.8);
                }
                return;
            }
            item.turnIn -= dt;
            if (item.turnIn <= 0) chooseDirection(item);
            item.vx += (item.tx - item.vx) * dt * .8;
            item.vy += (item.ty - item.vy) * dt * .8;
            item.x += item.vx * dt;
            item.y += item.vy * dt;
            item.spin += (item.spinTarget - item.spin) * dt;
            item.rotation += item.spin * dt;
            if (item.kind !== 'planet' && Math.abs(item.rotation) > 24) {
                item.spinTarget = -Math.sign(item.rotation) * random(2, 6);
            }
            bounce(item);
        });
        updateLasers(dt);
        updateExplosions(dt);
        if (!menuOpen) separateWorlds(dt);
        entities.forEach(item => { bounce(item); paint(item); });
        frame = requestAnimationFrame(tick);
    }

    function updatePlayback() {
        if (!canRun()) {
            cancelAnimationFrame(frame);
            frame = 0;
            previousTime = 0;
            clearEffects();
            if (reducedMotion.matches) {
                entities.filter(item => item.kind === 'raider').forEach(item => {
                    item.flight = null;
                    item.waitIn = random(3, 6);
                    item.element.classList.remove('is-flying');
                });
            }
        } else if (!frame) frame = requestAnimationFrame(tick);
    }
    const resize = new ResizeObserver(() => {
        clearEffects();
        entities.forEach(item => { measure(item); paint(item); });
        previousTime = 0;
    });
    [...new Set(entities.map(item => item.parent))].forEach(parent => resize.observe(parent));
    document.addEventListener('spaceflightchange', event => {
        suspended = !!event.detail?.suspended;
        updatePlayback();
    });
    document.addEventListener('worldmotionchange', updatePlayback);
    document.addEventListener('visibilitychange', updatePlayback);
    reducedMotion.addEventListener('change', updatePlayback);
    window.addEventListener('pageshow', () => { previousTime = 0; updatePlayback(); });
    updatePlayback();
})();
