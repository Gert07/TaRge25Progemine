(() => {
    const scene = document.querySelector('.playground');
    if (!scene) return;
    const objects = [...scene.querySelectorAll('.world-object')];
    const worlds = scene.querySelector('.floating-worlds');
    const header = scene.querySelector('.orbit-header');
    const veil = document.querySelector('.navigation-veil');
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const animations = new Set();
    let travelling = false;
    let generation = 0;
    let destination = null;

    function animate(element, keyframes, options) {
        const animation = element.animate(keyframes, { fill: 'forwards', ...options });
        animations.add(animation);
        if (document.hidden) animation.pause();
        return animation;
    }
    const finished = list => Promise.all(list.map(animation => animation.finished.catch(() => {})));
    function setFlight(suspended) {
        document.dispatchEvent(new CustomEvent('spaceflightchange', { detail: { suspended } }));
    }
    function resetScene() {
        ++generation;
        animations.forEach(animation => animation.cancel());
        animations.clear();
        travelling = false;
        destination = null;
        delete scene.dataset.effect;
        scene.removeAttribute('aria-busy');
        scene.style.removeProperty('transform-origin');
        worlds.inert = false;
        header.inert = false;
        setFlight(false);
    }
    function navigate(url) {
        try { sessionStorage.setItem('world-arrival', JSON.stringify({ path: url.pathname, time: Date.now() })); }
        catch { /* Storage is optional; the link still works. */ }
        try { location.assign(url.href); }
        catch { resetScene(); }
    }
    function ordinaryClick(event, link) {
        return !event.defaultPrevented && event.button === 0 && !event.ctrlKey && !event.metaKey &&
            !event.shiftKey && !event.altKey && !link.hasAttribute('download') &&
            (!link.target || link.target === '_self');
    }
    async function enterWorld(event, object, link) {
        if (!ordinaryClick(event, link)) return;
        event.preventDefault();
        if (travelling) return;
        travelling = true;
        const token = ++generation;
        const url = new URL(link.href);
        destination = url;
        setFlight(true);
        scene.dataset.effect = 'navigation';
        scene.setAttribute('aria-busy', 'true');
        worlds.inert = true;
        header.inert = true;
        try {
            const bounds = scene.getBoundingClientRect();
            const target = object.getBoundingClientRect();
            const x = target.left + target.width / 2 - bounds.left;
            const y = target.top + target.height / 2 - bounds.top;
            const duration = reducedMotion.matches ? 180 : 720;
            scene.style.transformOrigin = `${x}px ${y}px`;
            const travel = [];
            if (!reducedMotion.matches) {
                travel.push(animate(scene, [
                    { transform: 'translate(0px, 0px) scale(1)', filter: 'blur(0px)' },
                    { transform: `translate(${bounds.width / 2 - x}px, ${bounds.height / 2 - y}px) scale(2.1)`, filter: 'blur(.5px)' }
                ], { duration, easing: 'cubic-bezier(.4,0,.2,1)' }));
                [header, scene.querySelector('.orbit-footer')].forEach(element => {
                    travel.push(animate(element, [{ opacity: 1 }, { opacity: 0 }], { duration: 260 }));
                });
                objects.filter(item => item !== object).forEach(element => {
                    travel.push(animate(element, [{ opacity: 1 }, { opacity: 0 }], { duration: 350 }));
                });
            }
            travel.push(animate(veil, reducedMotion.matches ? [{ opacity: 0 }, { opacity: 1 }] : [
                { opacity: 0, offset: 0 }, { opacity: 0, offset: .18 },
                { opacity: .8, offset: .53 }, { opacity: 1, offset: .72 }, { opacity: 1, offset: 1 }
            ], { duration, easing: 'linear' }));
            await finished(travel);
            if (token === generation) navigate(url);
        } catch {
            if (token === generation) { resetScene(); navigate(url); }
        }
    }
    objects.forEach(object => object.addEventListener('click', event => enterWorld(event, object, object)));
    document.addEventListener('visibilitychange', () => {
        animations.forEach(animation => {
            if (animation.playState === 'finished' || animation.playState === 'idle') return;
            if (document.hidden) animation.pause();
            else if (animation.playState === 'paused') animation.play();
        });
    });
    reducedMotion.addEventListener('change', () => {
        if (!travelling || !destination) return;
        const url = destination;
        resetScene();
        navigate(url);
    });
    window.addEventListener('pageshow', event => { if (event.persisted) resetScene(); });
})();
