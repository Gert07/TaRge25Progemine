(() => {
    const $ = window.jQuery;
    if ($?.validator) {
        $.extend($.validator.messages, {
            required: 'See väli on kohustuslik.',
            remote: 'Kontrolli sisestatud väärtust.',
            email: 'Sisesta korrektne e-posti aadress.',
            url: 'Sisesta korrektne veebiaadress.',
            date: 'Sisesta korrektne kuupäev.',
            dateISO: 'Sisesta kuupäev kujul AAAA-KK-PP.',
            number: 'Sisesta arv.',
            digits: 'Kasuta ainult numbreid.',
            equalTo: 'Sisesta sama väärtus uuesti.',
            maxlength: $.validator.format('Sisesta kuni {0} märki.'),
            minlength: $.validator.format('Sisesta vähemalt {0} märki.'),
            rangelength: $.validator.format('Sisesta {0} kuni {1} märki.'),
            range: $.validator.format('Väärtus peab jääma vahemikku {0} kuni {1}.'),
            max: $.validator.format('Väärtus võib olla kuni {0}.'),
            min: $.validator.format('Väärtus peab olema vähemalt {0}.'),
            step: $.validator.format('Kasuta sammu {0}.')
        });
    }
    document.querySelectorAll('input[data-val-number]').forEach(input => {
        input.setAttribute('data-val-number', 'Sisesta arv.');
    });
    document.querySelectorAll('input[data-val-date]').forEach(input => {
        input.setAttribute('data-val-date', 'Sisesta korrektne kuupäev.');
    });
    document.querySelectorAll('input, textarea, select').forEach(field => {
        field.addEventListener('input', () => field.setCustomValidity(''));
        field.addEventListener('invalid', () => {
            const state = field.validity;
            let message = 'Kontrolli sisestatud väärtust.';
            if (state.valueMissing) message = 'See väli on kohustuslik.';
            else if (state.badInput) message = 'Sisesta korrektne arv.';
            else if (state.typeMismatch) message = field.type === 'email' ? 'Sisesta korrektne e-posti aadress.' : 'Sisesta korrektne aadress.';
            else if (state.rangeUnderflow) message = `Väärtus peab olema vähemalt ${field.min}.`;
            else if (state.rangeOverflow) message = `Väärtus võib olla kuni ${field.max}.`;
            else if (state.stepMismatch) message = 'Sisesta sobiva sammuga arv.';
            else if (state.tooLong) message = `Sisesta kuni ${field.maxLength} märki.`;
            else if (state.tooShort) message = `Sisesta vähemalt ${field.minLength} märki.`;
            field.setCustomValidity(message);
        });
    });
})();
