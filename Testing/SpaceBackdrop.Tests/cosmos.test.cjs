const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root, 'TaRge25Shop/wwwroot/js/cosmos.js'), 'utf8');
const markup = fs.readFileSync(path.join(root, 'TaRge25Shop/Views/Shared/_SpaceBackdrop.cshtml'), 'utf8');

function scene({ width = 1280, height = 820, viewportHeight = height, hitChance, reduced = false } = {}) {
    let time = 0, frameId = 0, seed = 42, resize;
    const frames = new Map(), events = {}, mediaEvents = {};
    const stats = { attacks: 0, impacts: 0, attackTimes: [], impactTimes: [], passes: [], impactPoints: [], maxEffects: 0 };
    stats.returnFire = [];
    stats.flightBounds = { top: Infinity, bottom: -Infinity };
    const media = { matches: reduced, addEventListener(name, fn) { mediaEvents[name] = fn; } };
    const emit = (name, detail) => (events[name] || []).forEach(fn => fn({ detail }));
    class Element {
        constructor() {
            this.style = { setProperty() {} };
            this.dataset = {};
            this.children = new Set();
            const classes = new Set();
            this.classList = {
                add: name => {
                    if (name === 'is-flying' && !classes.has(name)) {
                        this.pass = { start: time, x: [], actorId: this.actorId };
                        stats.passes.push(this.pass);
                    }
                    classes.add(name);
                },
                remove: name => {
                    if (name === 'is-flying' && classes.has(name)) this.pass.end = time;
                    classes.delete(name);
                },
                contains: name => classes.has(name)
            };
        }
        append(...elements) {
            elements.forEach(element => {
                this.children.add(element);
                element.parentElement = this;
                if (element.className === 'laser-bolt laser-attack') {
                    stats.attacks++;
                    stats.attackTimes.push(time);
                }
                if (element.className === 'laser-bolt laser-imperial') {
                    stats.returnFire.push({
                        angle: Number(element.style.transform.match(/rotate\(([-\d.]+)deg/)[1]),
                        visibleTargets: raiders.filter(fighter => fighter.classList.contains('is-flying') && position(fighter).x > 0 && position(fighter).x < background.clientWidth).length
                    });
                }
                if (element.className === 'hull-explosion') {
                    stats.impacts++;
                    stats.impactTimes.push(time);
                    stats.impactPoints.push({ point: position(element), target: position(destroyer) });
                }
            });
        }
        remove() { this.parentElement?.children.delete(this); }
    }
    const background = new Element();
    background.clientWidth = width;
    background.clientHeight = height;
    const ships = [...markup.matchAll(/<div\b[^>]*data-drift="(?:ship|raider)"[^>]*>/g)].map(([tag], index) => {
        const element = new Element();
        element.actorId = index;
        for (const [, key, value] of tag.matchAll(/data-([\w-]+)="([^"]*)"/g)) {
            element.dataset[key.replace(/-([a-z])/g, (_, letter) => letter.toUpperCase())] = value;
        }
        element.parentElement = background;
        if (element.dataset.ship === 'destroyer') {
            element.offsetWidth = Math.min(315, Math.max(161, width * .203));
            element.offsetHeight = element.offsetWidth * 1675 / 1880;
        } else {
            element.offsetWidth = tag.includes('flyby-wingman') ? Math.min(115.5, Math.max(59.5, width * .077)) : Math.min(136.5, Math.max(70, width * .091));
            element.offsetHeight = element.offsetWidth * 2153 / 4032;
        }
        if (element.dataset.drift === 'raider' && hitChance !== undefined) element.dataset.hitChance = hitChance;
        return element;
    });
    const destroyer = ships.find(ship => ship.dataset.ship === 'destroyer');
    const raiders = ships.filter(ship => ship.dataset.drift === 'raider');
    const raider = raiders[0];
    const document = {
        hidden: false,
        querySelectorAll(selector) { return selector === '[data-drift]' ? ships : []; },
        querySelector() { return null; },
        createElement() { return new Element(); },
        addEventListener(name, fn) { (events[name] ||= []).push(fn); }
    };
    const math = Object.create(Math);
    math.random = () => ((seed = (1664525 * seed + 1013904223) >>> 0) / 2 ** 32);
    vm.runInNewContext(source, {
        document, Math: math, innerWidth: width,
        window: { innerHeight: viewportHeight, matchMedia() { return media; }, addEventListener() {} },
        ResizeObserver: class { constructor(fn) { resize = fn; } observe() {} },
        requestAnimationFrame(fn) { frames.set(++frameId, fn); return frameId; },
        cancelAnimationFrame(id) { frames.delete(id); }
    });
    function advance(seconds) {
        for (let index = 0; index < Math.ceil(seconds * 60); index++) {
            time += 1000 / 60;
            const pending = [...frames.values()];
            frames.clear();
            pending.forEach(fn => fn(time));
            raiders.forEach(fighter => {
                if (fighter.classList.contains('is-flying')) {
                    const point = position(fighter);
                    fighter.pass.x.push(point.x);
                    const angle = Number(fighter.style.transform.match(/rotate\(([-\d.]+)deg/)[1]) * Math.PI / 180;
                    const halfHeight = (Math.abs(Math.cos(angle)) * fighter.offsetHeight + Math.abs(Math.sin(angle)) * fighter.offsetWidth) / 2;
                    stats.flightBounds.top = Math.min(stats.flightBounds.top, point.y - halfHeight);
                    stats.flightBounds.bottom = Math.max(stats.flightBounds.bottom, point.y + halfHeight);
                }
            });
            stats.maxEffects = Math.max(stats.maxEffects, background.children.size);
        }
    }
    return { background, ships, destroyer, raider, document, frames, stats, advance,
        resize: () => resize(),
        hide(hidden) { document.hidden = hidden; emit('visibilitychange'); },
        suspend(suspended) { emit('spaceflightchange', { suspended }); },
        reduce(value) { media.matches = value; mediaEvents.change(); }
    };
}

function position(element) {
    const match = element.style.transform.match(/translate3d\(([-\d.]+)px, ([-\d.]+)px/);
    return { x: Number(match[1]), y: Number(match[2]) };
}

test('flybys enter from the right, exit left and repeat at varying intervals', () => {
    const s = scene();
    s.advance(180);
    const passes = s.stats.passes.filter(pass => pass.end);
    assert(passes.length > 5);
    assert(passes[0].start >= 3000 && passes[0].start <= 6100);
    for (const pass of passes) {
        assert(pass.x[0] > s.background.clientWidth);
        assert(pass.x.at(-1) < 0);
        assert(pass.x.every((x, index) => index === 0 || x < pass.x[index - 1]));
    }
    const gaps = [...new Set(passes.map(pass => pass.actorId))].flatMap(id => {
        const flights = passes.filter(pass => pass.actorId === id);
        return flights.slice(1).map((pass, i) => pass.start - flights[i].end);
    });
    assert(gaps.every(gap => gap >= 8000 && gap <= 18100));
    assert(new Set(gaps.map(gap => Math.round(gap / 100))).size > 1);
});

test('zero hit chance fires misses without any hull explosions', () => {
    const s = scene({ hitChance: 0 });
    s.advance(180);
    assert(s.stats.attacks > 10);
    assert.equal(s.stats.impacts, 0);
});

test('guaranteed hits explode on the hull only after the shot travels', () => {
    const s = scene({ hitChance: 1 });
    s.advance(180);
    assert(s.stats.impacts > 10);
    assert(s.stats.impacts <= s.stats.attacks);
    assert(s.stats.impactTimes.every((time, index) => time > s.stats.attackTimes[index] + 50));
    for (const impact of s.stats.impactPoints) {
        assert(Math.abs(impact.point.x - impact.target.x) < s.destroyer.offsetWidth * .24);
        assert(Math.abs(impact.point.y - impact.target.y) < s.destroyer.offsetHeight * .24);
    }
    assert(s.stats.maxEffects < 35, 'expired projectiles and explosions must be removed');
});

test('default hit chance produces both hits and misses at approximately 40 percent', () => {
    const s = scene();
    s.advance(1200);
    const ratio = s.stats.impacts / s.stats.attacks;
    assert(s.stats.attacks > 100);
    assert(ratio > .3 && ratio < .5, `observed hit ratio: ${ratio}`);
});

test('mobile flybys still fire, and resize and pause clean up effects', () => {
    const s = scene({ width: 390, hitChance: 1 });
    s.advance(60);
    assert(s.stats.attacks > 0);
    assert(s.stats.impacts > 0);
    s.resize();
    assert.equal(s.background.children.size, 0);
    s.hide(true);
    assert.equal(s.frames.size, 0);
    const paused = s.raider.style.transform;
    s.advance(10);
    assert.equal(s.raider.style.transform, paused);
    s.hide(false);
    assert.equal(s.frames.size, 1);
    s.suspend(true);
    assert.equal(s.frames.size, 0);
    assert.equal(s.background.children.size, 0);
    s.suspend(false);
    assert.equal(s.frames.size, 1);
    s.reduce(true);
    assert.equal(s.frames.size, 0);
    assert(!s.raider.classList.contains('is-flying'));
});

test('initial reduced motion keeps the background quiet', () => {
    const s = scene({ reduced: true });
    s.advance(60);
    assert.equal(s.stats.passes.length, 0);
    assert.equal(s.background.children.size, 0);
    assert.equal(s.frames.size, 0);
});

test('fighters including their banked wings stay in the visible top third', () => {
    for (const size of [
        { width: 390, height: 820, viewportHeight: 594 },
        { width: 1280, height: 720, viewportHeight: 720 },
        { width: 1024, height: 820, viewportHeight: 320 }
    ]) {
        const s = scene(size);
        s.advance(120);
        assert(s.stats.flightBounds.top >= 0);
        assert(s.stats.flightBounds.bottom <= size.viewportHeight / 3);
    }
});

test('Destroyer return fire aims both ways and only fires at visible fighters', () => {
    const s = scene();
    s.advance(3);
    assert.equal(s.stats.returnFire.length, 0);
    s.advance(177);
    assert(s.stats.returnFire.length > 10);
    assert(s.stats.returnFire.every(shot => shot.visibleTargets > 0));
    assert(Math.cos(s.stats.returnFire[0].angle * Math.PI / 180) > 0);
    assert(s.stats.returnFire.some(shot => Math.cos(shot.angle * Math.PI / 180) < 0));
});
