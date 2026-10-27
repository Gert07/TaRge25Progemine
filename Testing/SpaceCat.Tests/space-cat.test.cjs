const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const source = fs.readFileSync(path.resolve(__dirname, '../../TaRge25Shop/wwwroot/js/space-cat.js'), 'utf8');

function scene({ present = true, hidden = false, reduced = false, width = 1280, height = 720 } = {}) {
    let now = 0, id = 0, seed = 12;
    const timers = new Map(), flights = [];
    function element() {
        const events = {};
        return {
            addEventListener(name, fn) { (events[name] ||= []).push(fn); },
            emit(name, event = {}) { (events[name] || []).forEach(fn => fn(event)); }
        };
    }
    const setTimer = (fn, delay) => { timers.set(++id, { fn, at: now + delay }); return id; };
    const cat = element();
    cat.hidden = true;
    cat.style = {};
    const classes = new Set();
    cat.classList = { add: name => classes.add(name), remove: name => classes.delete(name) };
    let captured = null;
    cat.setPointerCapture = id => { captured = id; };
    cat.hasPointerCapture = id => captured === id;
    cat.releasePointerCapture = id => {
        if (captured === id) { captured = null; cat.emit('lostpointercapture', { pointerId: id }); }
    };
    cat.offsetWidth = Math.min(165, Math.max(105, width * .13));
    function pose() {
        const live = flights.at(-1);
        if (live && live.animation.playState !== 'idle') {
            const progress = Math.min(1, live.animation.currentTime / live.options.duration);
            const index = progress <= .5 ? 0 : 1;
            const amount = index === 0 ? progress * 2 : (progress - .5) * 2;
            const start = point(live.keyframes[index]), end = point(live.keyframes[index + 1]);
            return { x: start.x + (end.x - start.x) * amount, y: start.y + (end.y - start.y) * amount, angle: start.angle + (end.angle - start.angle) * amount };
        }
        return point({ transform: cat.style.transform });
    }
    cat.getBoundingClientRect = () => {
        const { x, y, angle } = pose();
        const radians = angle * Math.PI / 180;
        const size = cat.offsetWidth * (Math.abs(Math.cos(radians)) + Math.abs(Math.sin(radians)));
        return { left: x - size / 2, top: y - size / 2, width: size, height: size };
    };
    cat.animate = (keyframes, options) => {
        let resolve, reject, timer, deadline, remaining = options.duration;
        const animation = {
            playState: 'paused',
            get currentTime() { return options.duration - (this.playState === 'running' ? Math.max(0, deadline - now) : this.playState === 'finished' ? 0 : remaining); },
            finished: new Promise((yes, no) => { resolve = yes; reject = no; }),
            play() {
                this.playState = 'running';
                deadline = now + remaining;
                timer = setTimer(() => { this.playState = 'finished'; resolve(); }, remaining);
            },
            pause() {
                remaining = deadline - now;
                timers.delete(timer);
                this.playState = 'paused';
            },
            cancel() {
                timers.delete(timer);
                this.playState = 'idle';
                reject(new Error('cancelled'));
            }
        };
        animation.play();
        flights.push({ time: now, keyframes, options, animation });
        return animation;
    };
    const document = element(), window = element(), media = element();
    document.hidden = hidden;
    document.querySelector = () => present ? cat : null;
    media.matches = reduced;
    window.innerWidth = width;
    window.innerHeight = height;
    window.matchMedia = () => media;
    const math = Object.create(Math);
    math.random = () => ((seed = (1664525 * seed + 1013904223) >>> 0) / 2 ** 32);
    vm.runInNewContext(source, {
        document, window, Math: math, Date: { now: () => now },
        setTimeout: setTimer, clearTimeout: timer => timers.delete(timer),
        requestAnimationFrame: fn => setTimer(() => fn(now), 1000 / 60),
        cancelAnimationFrame: timer => timers.delete(timer),
        getComputedStyle: () => {
            const radians = pose().angle * Math.PI / 180;
            return { transform: `matrix(${Math.cos(radians)},${Math.sin(radians)},0,0,0,0)` };
        },
        DOMMatrixReadOnly: class {
            constructor(value) { const values = value.slice(7, -1).split(',').map(Number); this.a = values[0]; this.b = values[1]; }
        }
    });
    async function advance(milliseconds) {
        const end = now + milliseconds;
        let count = 0;
        while (timers.size) {
            const [key, next] = [...timers].sort((a, b) => a[1].at - b[1].at)[0];
            if (next.at > end) break;
            assert(++count < 10000, 'timer must not spin without a delay');
            now = next.at;
            timers.delete(key);
            next.fn();
            await Promise.resolve();
            await Promise.resolve();
        }
        now = end;
    }
    const delay = () => Math.min(...[...timers.values()].map(timer => timer.at - now));
    const pointerEvent = (name, x, y) => cat.emit(name, { pointerId: 1, button: 0, isPrimary: true, clientX: x, clientY: y, preventDefault() {} });
    return { cat, document, window, media, timers, flights, advance, delay, pose, pointerEvent,
        next() { return advance(delay()); },
        visibility(value) { document.hidden = value; document.emit('visibilitychange'); },
        reduce(value) { media.matches = value; media.emit('change'); }
    };
}

function point(frame) {
    const match = frame.transform.match(/translate3d\(([-\d.]+)px, ([-\d.]+)px/);
    return { x: Number(match[1]), y: Number(match[2]), angle: Number(frame.transform.match(/rotate\(([-\d.]+)deg/)[1]) };
}

test('first event waits 5–60 seconds, crosses the viewport fast and rolls three times', async () => {
    const s = scene();
    assert(s.delay() >= 5000 && s.delay() <= 60000);
    await s.advance(4999);
    assert(s.cat.hidden);
    assert.equal(s.flights.length, 0);
    await s.next();
    assert(!s.cat.hidden);
    const flight = s.flights[0];
    assert.equal(flight.options.duration, 2200);
    assert.equal(flight.options.easing, 'linear');
    const start = point(flight.keyframes[0]), end = point(flight.keyframes.at(-1));
    assert(start.x > 1280 && end.x < 0);
    assert(Math.abs(end.angle - start.angle + 1080) < .001);
    await s.advance(2200);
    assert(s.cat.hidden);
});

test('events use fresh random 5–60 second intervals without overlapping', async () => {
    const s = scene();
    await s.advance(1200000);
    assert(s.flights.length > 15);
    const intervals = s.flights.slice(1).map((flight, index) => flight.time - s.flights[index].time);
    assert(intervals.every(interval => interval >= 5000 && interval <= 60000));
    assert(new Set(intervals.map(interval => Math.round(interval))).size > 10);
    assert.equal(s.timers.size, 1);
    assert(new Set(s.flights.map(flight => point(flight.keyframes[0]).y)).size > 1);
});

test('spinning cat stays vertically inside desktop and mobile viewports', async () => {
    for (const [width, height] of [[1280, 720], [390, 594], [844, 320]]) {
        const s = scene({ width, height });
        await s.next();
        const radius = s.cat.offsetWidth * Math.SQRT2 / 2;
        for (const frame of s.flights[0].keyframes) {
            const { y } = point(frame);
            assert(y >= radius);
            assert(y <= height - radius);
        }
    }
});

test('hidden tab pauses both the countdown and an active flight', async () => {
    const s = scene();
    const initialDelay = s.delay();
    await s.advance(2000);
    s.visibility(true);
    await s.advance(120000);
    assert.equal(s.flights.length, 0);
    s.visibility(false);
    await s.advance(initialDelay - 2000);
    assert.equal(s.flights.length, 1);
    await s.advance(1000);
    s.visibility(true);
    assert.equal(s.flights[0].animation.playState, 'paused');
    await s.advance(120000);
    assert(!s.cat.hidden);
    s.visibility(false);
    await s.advance(1200);
    assert(s.cat.hidden);
    await s.next();
    assert.equal(s.flights.length, 2);
});

test('reduced motion suppresses the event and cancels active rolling safely', async () => {
    const s = scene({ reduced: true });
    await s.advance(120000);
    assert.equal(s.flights.length, 0);
    s.reduce(false);
    await s.next();
    assert(!s.cat.hidden);
    s.reduce(true);
    await s.advance(120000);
    assert(s.cat.hidden);
    assert.equal(s.timers.size, 0);
    s.reduce(false);
    await s.next();
    assert.equal(s.flights.length, 2);
});

test('Escape cancels a pass, and back/forward restoration does not duplicate events', async () => {
    const s = scene();
    await s.next();
    s.document.emit('keydown', { key: 'Escape' });
    await Promise.resolve();
    assert(s.cat.hidden);
    assert.equal(s.timers.size, 1);
    s.window.emit('pagehide');
    await s.advance(120000);
    s.window.emit('pageshow');
    s.window.emit('pageshow');
    assert.equal(s.timers.size, 1);
    await s.next();
    assert.equal(s.flights.length, 2);
    const home = scene({ present: false });
    await home.advance(180000);
    assert.equal(home.timers.size, 0);
    assert.equal(home.flights.length, 0);
});

test('grabbing stops the flyby without jumping, and a slow release keeps the cat drifting', async () => {
    const s = scene();
    await s.next();
    await s.advance(1100);
    const before = s.pose();
    s.pointerEvent('pointerdown', before.x + 10, before.y + 5);
    const caught = s.pose();
    assert(Math.abs(caught.x - before.x) < .001);
    assert(Math.abs(caught.y - before.y) < .001);
    assert.equal(s.flights[0].animation.playState, 'idle');
    assert(s.cat.hasPointerCapture(1));
    await s.advance(3000);
    assert.deepEqual(s.pose(), caught, 'holding must stop both movement and rolling');
    s.pointerEvent('pointermove', before.x + 70, before.y + 35);
    assert(Math.abs(s.pose().x - before.x - 60) < .001);
    await s.advance(300);
    s.pointerEvent('pointerup', before.x + 70, before.y + 35);
    assert(!s.cat.hasPointerCapture(1));
    const released = s.pose();
    await s.advance(1000);
    assert(Math.hypot(s.pose().x - released.x, s.pose().y - released.y) < 20);
    await s.advance(90000);
    assert(!s.cat.hidden, 'caught cat should stay instead of finishing its cancelled flyby');
    assert.equal(s.flights.length, 1, 'no duplicate cat may appear while this one is caught');
    assert(s.pose().x > 0 && s.pose().x < 1280);
    assert(s.pose().y > 0 && s.pose().y < 720);
});

test('flicking through any edge restarts a fresh 5–60 second appearance countdown', async () => {
    for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
        const s = scene();
        await s.next();
        await s.advance(1100);
        const start = s.pose();
        s.pointerEvent('pointerdown', start.x, start.y);
        await s.advance(20);
        s.pointerEvent('pointermove', start.x + dx * 80, start.y + dy * 80);
        await s.advance(20);
        s.pointerEvent('pointermove', start.x + dx * 160, start.y + dy * 160);
        await s.advance(20);
        s.pointerEvent('pointerup', start.x + dx * 220, start.y + dy * 220);
        s.cat.emit('click', { detail: 1 });
        for (let step = 0; step < 300 && !s.cat.hidden; step++) await s.advance(1000 / 60);
        assert(s.cat.hidden, `throw should exit toward ${dx},${dy}`);
        assert(s.delay() >= 4980 && s.delay() <= 60000);
        assert.equal(s.timers.size, 1);
        await s.next();
        assert.equal(s.flights.length, 2);
        assert(!s.cat.hidden);
    }
});

test('pointer cancellation and lost capture release into a safe slow drift', async () => {
    for (const event of ['pointercancel', 'lostpointercapture']) {
        const s = scene();
        await s.next();
        await s.advance(1100);
        const start = s.pose();
        s.pointerEvent('pointerdown', start.x, start.y);
        await s.advance(20);
        s.pointerEvent('pointermove', start.x + 100, start.y);
        s.cat.emit(event, { pointerId: 1 });
        const released = s.pose();
        await s.advance(1000);
        assert(!s.cat.hidden);
        assert(!s.cat.hasPointerCapture(1));
        assert(Math.hypot(s.pose().x - released.x, s.pose().y - released.y) < 20);
    }
});

test('leaving the tab while dragging releases capture and pauses the slow drift', async () => {
    const s = scene();
    await s.next();
    await s.advance(1100);
    const start = s.pose();
    s.pointerEvent('pointerdown', start.x, start.y);
    s.visibility(true);
    assert(!s.cat.hasPointerCapture(1));
    const paused = s.pose();
    await s.advance(60000);
    assert.deepEqual(s.pose(), paused);
    assert.equal(s.timers.size, 0);
    s.visibility(false);
    await s.advance(1000);
    assert(!s.cat.hidden);
    assert.equal(s.timers.size, 1);
});

test('keyboard activation can catch the cat and reduced motion clears a caught cat', async () => {
    const s = scene();
    await s.next();
    await s.advance(1100);
    s.cat.emit('click', { detail: 0 });
    await s.advance(10000);
    assert(!s.cat.hidden);
    assert.equal(s.flights.length, 1);
    s.reduce(true);
    await s.advance(60000);
    assert(s.cat.hidden);
    assert.equal(s.timers.size, 0);
});

test('a quick mouse click also catches a flyby', async () => {
    const s = scene();
    await s.next();
    await s.advance(1100);
    s.cat.emit('click', { detail: 1 });
    await s.advance(10000);
    assert(!s.cat.hidden);
    assert.equal(s.flights.length, 1);
});
