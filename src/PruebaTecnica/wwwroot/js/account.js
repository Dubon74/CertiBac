/*
 * Validación en el cliente para registro e inicio de sesión (UX inmediata).
 * La validación REAL se repite siempre en el servidor (DataAnnotations + SP).
 */
(function ($) {
    'use strict';

    $('form.needs-validation').on('submit', function (event) {
        const $form = $(this);
        let valido = true;

        $form.find('input[data-val-required]').each(function () {
            const $input = $(this);
            const nombre = $input.attr('name');
            const $mensaje = $form.find('span[data-valmsg-for="' + nombre + '"]');
            const valor = $input.val() || '';
            let error = '';

            if (valor.trim() === '') {
                error = $input.attr('data-val-required');
            } else if ($input.attr('data-val-length-min') && valor.length < Number($input.attr('data-val-length-min'))) {
                error = $input.attr('data-val-length');
            } else if ($input.attr('data-val-equalto-other')) {
                const otro = $form.find('input[name="' + $input.attr('data-val-equalto-other').replace('*.', '') + '"]');
                if (otro.length && otro.val() !== valor) {
                    error = $input.attr('data-val-equalto');
                }
            }

            $input.toggleClass('is-invalid', error !== '');
            $mensaje.text(error);

            if (error !== '') {
                valido = false;
            }
        });

        if (!valido) {
            event.preventDefault();
        }
    });
})(jQuery);
