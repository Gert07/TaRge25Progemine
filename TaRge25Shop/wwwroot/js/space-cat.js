(() => {
    const cat = document.querySelector('[data-space-cat]');
    if (!cat) return;
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const duration = 2200;
    const random = (min, max) => min + Math.random() * (max - min);
    const nextInterval = () => random(5000, 60000);
    const clamp = (value, min, max) => Math.max(min, Math.min(max, value));
    let timer = null;
    let deadline = 0;
    let remaining = nextInterval();
    let animation = null;
    let generation = 0;
    let pageAway = false;
    let mode = 'waiting';
    let frame = null;
    let previousFrame = 0;
    let pointer = null;
    let samples = [];
    const body = { x: 0, y: 0, angle: 0, vx: 0, vy: 0, spin: 0, phase: 0, turnIn: 0 };
    const canRun = () => !document.hidden && !pageAway && !reducedMotion.matches;

    function paint() {
        cat.style.transform = `translate3d(${body.x}px, ${body.y}px, 0) translate(-50%, -50%) rotate(${body.angle}deg)`;
    }

    function chooseDrift() {
        const direction = random(0, Math.PI * 2);
        const speed = random(10, 18);
        body.tx = Math.cos(direction) * speed;
        body.ty = Math.sin(direction) * speed;
        body.turnIn = random(3, 6);
    }

    function keepOnScreen() {
        const radius = cat.offsetWidth * Math.SQRT2 / 2 + 8;
        const xInset = Math.min(window.innerWidth / 2, radius);
        const yInset = Math.min(window.innerHeight / 2, radius);
        const x = clamp(body.x, xInset, window.innerWidth - xInset);
        const y = clamp(body.y, yInset, window.innerHeight - yInset);
        if (x !== body.x) {
            body.vx = Math.abs(body.vx) * (body.x < x ? 1 : -1);
            body.tx = Math.abs(body.tx) * (body.x < x ? 1 : -1);
        }
        if (y !== body.y) {
            body.vy = Math.abs(body.vy) * (body.y < y ? 1 : -1);
            body.ty = Math.abs(body.ty) * (body.y < y ? 1 : -1);
        }
        body.x = x;
        body.y = y;
    }

    function outsideScreen() {
        const radius = cat.offsetWidth * Math.SQRT2 / 2;
        return body.x + radius < 0 || body.x - radius > window.innerWidth || body.y + radius < 0 || body.y - radius > window.innerHeight;
    }

    function drift() {
        mode = 'drift';
        body.vx = 0;
        body.vy = 0;
        body.spin = 0;
        chooseDrift();
        keepOnScreen();
        paint();
    }

    function move(time) {
        frame = null;
        if (!canRun() || (mode !== 'drift' && mode !== 'throw')) return;
        const dt = previousFrame ? Math.min((time - previousFrame) / 1000, .05) : 0;
        previousFrame = time;
        if (mode === 'throw') {
            body.x += body.vx * dt;
            body.y += body.vy * dt;
            body.angle += body.spin * dt;
            body.vx *= Math.exp(-.15 * dt);
            body.vy *= Math.exp(-.15 * dt);
            if (outsideScreen()) {
                cancelFlight();
                resume();
                return;
            }
            if (Math.hypot(body.vx, body.vy) < 35) drift();
        } else {
            body.turnIn -= dt;
            if (body.turnIn <= 0) chooseDrift();
            body.vx += (body.tx - body.vx) * Math.min(1, dt * 1.2);
            body.vy += (body.ty - body.vy) * Math.min(1, dt * 1.2);
            body.x += body.vx * dt;
            body.y += body.vy * dt;
            body.phase += dt * .6;
            body.angle += (Math.sin(body.phase) * 3 - body.angle) * Math.min(1, dt * 2);
            keepOnScreen();
        }
        paint();
        frame = requestAnimationFrame(move);
    }

    function stopPointer() {
        const id = pointer?.id;
        pointer = null;
        cat.classList.remove('is-dragging');
        if (id !== undefined && cat.hasPointerCapture(id)) cat.releasePointerCapture(id);
    }

    function pause() {
        if (timer !== null) {
            remaining = Math.max(0, deadline - Date.now());
            clearTimeout(timer);
            timer = null;
        }
        if (animation?.playState === 'running') animation.pause();
        if (frame !== null) cancelAnimationFrame(frame);
        frame = null;
        previousFrame = 0;
        if (pointer) {
            stopPointer();
            drift();
        }
    }

    function resume() {
        if (!canRun()) return;
        if (mode === 'drag') return;
        if (mode === 'drift' || mode === 'throw') {
            if (frame === null) frame = requestAnimationFrame(move);
            return;
        }
        if (animation) {
            if (animation.playState === 'paused') animation.play();
            return;
        }
        if (timer !== null) return;
        deadline = Date.now() + remaining;
        timer = setTimeout(() => {
            timer = null;
            flyAcross();
        }, remaining);
    }

    function flyAcross() {
        if (!canRun()) return;
        mode = 'flyby';
        cat.hidden = false;
        const width = window.innerWidth;
        const height = window.innerHeight;
        const margin = cat.offsetWidth * Math.SQRT2 / 2 + 16;
        const inset = Math.min(height / 2, margin);
        const startY = clamp(height * random(.25, .75), inset, height - inset);
        const endY = clamp(startY + random(-70, 70), inset, height - inset);
        const middleY = clamp((startY + endY) / 2 + random(-35, 35), inset, height - inset);
        const roll = random(-25, 25);
        const transform = (x, y, angle) => `translate3d(${x}px, ${y}px, 0) translate(-50%, -50%) rotate(${angle}deg)`;
        const token = ++generation;
        animation = cat.animate([
            { transform: transform(width + margin, startY, roll) },
            { transform: transform(width / 2, middleY, roll - 540), offset: .5 },
            { transform: transform(-margin, endY, roll - 1080) }
        ], { duration, easing: 'linear', fill: 'forwards' });
        animation.finished.then(() => {
            if (token !== generation) return;
            animation.cancel();
            animation = null;
            cat.hidden = true;
            mode = 'waiting';
            // Choose a fresh 5–60 second interval between entrances.
            remaining = nextInterval() - duration;
            resume();
        }).catch(() => {
            // Cancellation during navigation or a preference change is expected.
        });
    }

    function cancelFlight() {
        pause();
        ++generation;
        animation?.cancel();
        animation = null;
        mode = 'waiting';
        cat.hidden = true;
        remaining = nextInterval();
    }

    function catchCat() {
        // Read the animated position before cancelling the flyby to avoid a jump.
        const bounds = cat.getBoundingClientRect();
        const matrix = new DOMMatrixReadOnly(getComputedStyle(cat).transform);
        body.x = bounds.left + bounds.width / 2;
        body.y = bounds.top + bounds.height / 2;
        body.angle = Math.atan2(matrix.b, matrix.a) * 180 / Math.PI;
        pause();
        ++generation;
        animation?.cancel();
        animation = null;
        body.vx = 0;
        body.vy = 0;
        body.spin = 0;
        paint();
    }

    function recordPointer(event) {
        const time = Date.now();
        samples.push({ x: event.clientX, y: event.clientY, time });
        samples = samples.filter(sample => time - sample.time <= 120);
    }

    function release(event) {
        if (pointer?.id !== event.pointerId) return;
        body.x = event.clientX - pointer.dx;
        body.y = event.clientY - pointer.dy;
        recordPointer(event);
        const first = samples[0], last = samples.at(-1);
        const elapsed = Math.max(16, last.time - first.time) / 1000;
        const vx = (last.x - first.x) / elapsed;
        const vy = (last.y - first.y) / elapsed;
        const speed = Math.hypot(vx, vy);
        stopPointer();
        if (outsideScreen()) cancelFlight();
        else if (speed >= 220) {
            mode = 'throw';
            const scale = Math.min(1, 1800 / speed);
            body.vx = vx * scale;
            body.vy = vy * scale;
            body.spin = clamp(vx * .03 + vy * .015, -100, 100);
        } else drift();
        paint();
        previousFrame = 0;
        resume();
    }

    cat.addEventListener('pointerdown', event => {
        if (cat.hidden || !canRun() || event.button !== 0 || event.isPrimary === false || pointer) return;
        event.preventDefault();
        catchCat();
        mode = 'drag';
        pointer = { id: event.pointerId, dx: event.clientX - body.x, dy: event.clientY - body.y };
        samples = [];
        recordPointer(event);
        cat.classList.add('is-dragging');
        cat.setPointerCapture(event.pointerId);
    });
    cat.addEventListener('pointermove', event => {
        if (pointer?.id !== event.pointerId) return;
        event.preventDefault();
        body.x = event.clientX - pointer.dx;
        body.y = event.clientY - pointer.dy;
        recordPointer(event);
        paint();
    });
    cat.addEventListener('pointerup', event => release(event));
    cat.addEventListener('pointercancel', event => {
        if (pointer?.id !== event.pointerId) return;
        stopPointer();
        drift();
        resume();
    });
    cat.addEventListener('lostpointercapture', event => {
        if (pointer?.id !== event.pointerId) return;
        stopPointer();
        drift();
        resume();
    });
    cat.addEventListener('click', event => {
        // A quick tap can also catch the flyby; a click after a throw must not stop it.
        if ((event.detail !== 0 && mode !== 'flyby') || pointer || cat.hidden || !canRun()) return;
        catchCat();
        drift();
        resume();
    });
    window.addEventListener('resize', () => {
        if (mode === 'drift') { keepOnScreen(); paint(); }
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && !cat.hidden) {
            cancelFlight();
            resume();
        }
    });
    document.addEventListener('visibilitychange', () => { if (document.hidden) pause(); else resume(); });
    window.addEventListener('pagehide', () => { pageAway = true; pause(); });
    window.addEventListener('pageshow', () => { pageAway = false; resume(); });
    reducedMotion.addEventListener('change', () => { cancelFlight(); resume(); });
    resume();
})();
