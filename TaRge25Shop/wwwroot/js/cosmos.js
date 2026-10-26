(() => {
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const random = (min, max) => min + Math.random() * (max - min);
    const clamp = (value, min, max) => Math.max(min, Math.min(max, value));
    const entities = [];
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

    function paint(item) {
        const rotation = item.kind === 'world' ? 0 : item.rotation;
        item.element.style.transform = `translate3d(${item.x.toFixed(2)}px, ${item.y.toFixed(2)}px, 0) translate(-50%, -50%) rotate(${rotation.toFixed(2)}deg)`;
        if (item.kind === 'world') item.element.style.setProperty('--art-rotation', `${item.rotation.toFixed(2)}deg`);
    }

    document.querySelectorAll('[data-drift]').forEach(element => {
        const kind = element.dataset.drift;
        const parent = kind === 'world' ? element.closest('.playground') : element.parentElement;
        const item = { element, parent, kind, held: false, rotation: random(-12, 12), spin: 0 };
        chooseDirection(item);
        item.vx = item.tx;
        item.vy = item.ty;
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
        if (!menuOpen) separateWorlds(dt);
        entities.forEach(item => { bounce(item); paint(item); });
        frame = requestAnimationFrame(tick);
    }

    function updatePlayback() {
        if (!canRun()) {
            cancelAnimationFrame(frame);
            frame = 0;
            previousTime = 0;
        } else if (!frame) frame = requestAnimationFrame(tick);
    }
    const resize = new ResizeObserver(() => {
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
