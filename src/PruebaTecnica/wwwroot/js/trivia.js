/*
 * Lógica de la partida en el navegador (jQuery + AJAX).
 * El cliente solo muestra preguntas y envía la respuesta elegida:
 * la calificación se hace SIEMPRE en el servidor.
 */
(function ($) {
    'use strict';

    const state = {
        categoriaId: null,
        categoriaNombre: '',
        partidaId: null,
        preguntas: [],
        indice: 0,
        bloqueado: false,
        ultimo: null
    };

    // ---------------------------- Utilidades ----------------------------

    function token() {
        return $('#antiforgery-form input[name="__RequestVerificationToken"]').val();
    }

    function post(url, data) {
        return $.ajax({
            url: url,
            method: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(data),
            headers: { RequestVerificationToken: token() }
        });
    }

    function mostrarPanel(nombre) {
        $('#panel-categorias, #panel-juego, #panel-resultado').addClass('d-none');
        $('#panel-' + nombre).removeClass('d-none');
    }

    function mostrarAlerta(mensaje) {
        $('#alerta').text(mensaje).removeClass('d-none');
    }

    function ocultarAlerta() {
        $('#alerta').addClass('d-none').text('');
    }

    function manejarError(xhr) {
        if (xhr.status === 401) {
            window.location.href = '/Account/Login';
            return;
        }

        const mensaje = xhr.responseJSON && xhr.responseJSON.mensaje
            ? xhr.responseJSON.mensaje
            : 'Ocurrió un error inesperado. Intenta de nuevo.';
        mostrarAlerta(mensaje);
    }

    // ------------------------------ Partida ------------------------------

    function iniciarPartida(categoriaId, categoriaNombre) {
        ocultarAlerta();
        $('.btn-categoria, #btn-repetir').prop('disabled', true);

        post('/api/trivia/iniciar', { categoriaId: categoriaId })
            .done(function (partida) {
                state.categoriaId = categoriaId;
                state.categoriaNombre = categoriaNombre;
                state.partidaId = partida.partidaId;
                state.preguntas = partida.preguntas;
                state.indice = 0;
                state.ultimo = null;

                mostrarPanel('juego');
                mostrarPregunta();
            })
            .fail(manejarError)
            .always(function () {
                $('.btn-categoria, #btn-repetir').prop('disabled', false);
            });
    }

    function mostrarPregunta() {
        const pregunta = state.preguntas[state.indice];
        const total = state.preguntas.length;

        state.bloqueado = false;

        $('#juego-categoria').text(state.categoriaNombre);
        $('#juego-progreso').text('Pregunta ' + (state.indice + 1) + ' de ' + total);
        $('#juego-barra').css('width', (state.indice / total * 100) + '%');
        $('#juego-pregunta').text(pregunta.texto);

        const $lista = $('#juego-respuestas').empty();
        pregunta.respuestas.forEach(function (respuesta, i) {
            $('<button type="button" class="list-group-item list-group-item-action respuesta"></button>')
                .attr('data-respuesta-id', respuesta.id)
                .text(String.fromCharCode(65 + i) + '. ' + respuesta.texto)   
                .appendTo($lista);
        });

        $('#juego-feedback').addClass('d-none').removeClass('alert-success alert-danger').text('');
        $('#btn-siguiente').addClass('d-none');
    }

    function responder($boton) {
        if (state.bloqueado) {
            return;
        }

        state.bloqueado = true;
        ocultarAlerta();

        const pregunta = state.preguntas[state.indice];
        const respuestaId = Number($boton.attr('data-respuesta-id'));

        post('/api/trivia/responder', {
            partidaId: state.partidaId,
            preguntaId: pregunta.id,
            respuestaId: respuestaId
        })
            .done(function (resultado) {
                state.ultimo = resultado;

                $('#juego-respuestas .respuesta').addClass('bloqueada');
                $('#juego-respuestas .respuesta[data-respuesta-id="' + resultado.respuestaCorrectaId + '"]')
                    .addClass('list-group-item-success');

                const $feedback = $('#juego-feedback').removeClass('d-none alert-success alert-danger');
                if (resultado.esCorrecta) {
                    $feedback.addClass('alert-success').text('¡Correcto!');
                } else {
                    $boton.addClass('list-group-item-danger');
                    $feedback.addClass('alert-danger').text('Incorrecto. La respuesta correcta está resaltada en verde.');
                }

                $('#juego-barra').css('width', ((state.indice + 1) / state.preguntas.length * 100) + '%');
                $('#btn-siguiente')
                    .text(resultado.finalizada ? 'Ver resultado' : 'Siguiente')
                    .removeClass('d-none')
                    .trigger('focus');
            })
            .fail(function (xhr) {
                state.bloqueado = false;   
                manejarError(xhr);
            });
    }

    function mostrarResultado() {
        const puntaje = state.ultimo ? state.ultimo.puntaje : 0;
        const total = state.preguntas.length;

        const mensajes = {
            3: ['', '¡Perfecto!'],
            2: ['', '¡Muy bien!'],
            1: ['', 'Puedes mejorar'],
            0: ['', '¡Sigue intentando!']
        };
        const [emoji, titulo] = mensajes[puntaje] || mensajes[0];

        $('#resultado-emoji').text(emoji);
        $('#resultado-titulo').text(titulo);
        $('#resultado-puntaje').text(puntaje);
        $('#resultado-total').text(total);

        mostrarPanel('resultado');
    }

    // ------------------------------ Eventos ------------------------------

    $(function () {
        $('.btn-categoria').on('click', function () {
            iniciarPartida(Number($(this).attr('data-categoria-id')), $(this).attr('data-categoria-nombre'));
        });

        $('#juego-respuestas').on('click', '.respuesta', function () {
            responder($(this));
        });

        $('#btn-siguiente').on('click', function () {
            if (state.ultimo && state.ultimo.finalizada) {
                mostrarResultado();
            } else {
                state.indice++;
                mostrarPregunta();
            }
        });

        $('#btn-repetir').on('click', function () {
            iniciarPartida(state.categoriaId, state.categoriaNombre);
        });

        $('#btn-otra').on('click', function () {
            ocultarAlerta();
            mostrarPanel('categorias');
        });
    });
})(jQuery);
