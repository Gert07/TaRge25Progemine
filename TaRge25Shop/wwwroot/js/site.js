(() => {
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const menus = [...document.querySelectorAll('.world-menu')];

    function updateMotion() {
        document.body.classList.toggle('motion-paused', reducedMotion.matches);
        document.body.classList.toggle('motion-suspended', document.hidden);
        document.dispatchEvent(new CustomEvent('worldmotionchange'));
    }

    document.addEventListener('keydown', event => {
        if (event.key !== 'Escape') return;
        menus.forEach(menu => {
            if (!menu.open) return;
            menu.open = false;
            menu.querySelector('summary').focus();
        });
    });
    document.addEventListener('pointerdown', event => {
        menus.forEach(menu => { if (menu.open && !menu.contains(event.target)) menu.open = false; });
    });
    menus.forEach(menu => menu.addEventListener('toggle', () => {
        document.dispatchEvent(new CustomEvent('worldmenuchange'));
    }));
    reducedMotion.addEventListener('change', updateMotion);
    document.addEventListener('visibilitychange', updateMotion);
    updateMotion();

    // Only a deliberate trip from a drifting object triggers the arrival zoom.
    try {
        const entry = JSON.parse(sessionStorage.getItem('world-arrival') || 'null');
        sessionStorage.removeItem('world-arrival');
        const main = document.querySelector('.world-main');
        if (main && entry?.path === location.pathname && Date.now() - entry.time < 15000) {
            const panels = [...main.querySelectorAll('.world-intro, .world-surface')];
            const arrivals = panels.map(panel => panel.animate(reducedMotion.matches ? [
                { opacity: 0 }, { opacity: 1 }
            ] : [
                { opacity: 0, transform: 'scale(1.06) translateY(16px)', filter: 'blur(5px)' },
                { opacity: 1, transform: 'scale(1) translateY(0)', filter: 'blur(0px)' }
            ], { duration: reducedMotion.matches ? 180 : 650, easing: 'cubic-bezier(.2,.7,.2,1)' }));
            window.addEventListener('pageshow', event => {
                if (event.persisted) arrivals.forEach(arrival => arrival.cancel());
            });
        }
    } catch {
        // Navigation and form pages also work when browser storage is unavailable.
    }
})();

/* File-picker labels are app text, independent of the browser's display language. */
(() => {
    document.querySelectorAll('input[type="file"]').forEach(input => {
        const wrapper = document.createElement('div');
        wrapper.className = 'file-picker';
        const button = document.createElement('button');
        button.type = 'button';
        button.className = 'btn btn-secondary';
        button.textContent = 'Vali pildid';
        button.disabled = input.disabled;
        button.addEventListener('click', () => input.click());
        const status = document.createElement('span');
        status.className = 'file-picker-status';
        status.setAttribute('aria-live', 'polite');
        status.id = `${input.id || 'files'}-status`;
        button.setAttribute('aria-describedby', status.id);
        function updateStatus() {
            const names = [...input.files].map(file => file.name);
            status.textContent = names.length ? names.join(', ') : 'Pilte pole valitud.';
        }
        input.addEventListener('change', updateStatus);
        updateStatus();
        wrapper.append(button, status);
        input.after(wrapper);
        input.hidden = true;
    });
})();
