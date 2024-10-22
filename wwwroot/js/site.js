$(document).ready(function () {
    // Lógica do login
    if ($('#loginPage').length > 0) {
        function isValidEmail(email) {
            var regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return regex.test(email);
        }

        function isValidPhone(phone) {
            var regex = /^\(\d{2}\)\s\d{5}-\d{4}$/;
            return regex.test(phone);
        }

        var emailDomains = ["gmail.com", "yahoo.com.br", "outlook.com", "hotmail.com"];

        // Quando o usuário digitar no campo de telefone
        $('#phoneInput').on('input', function () {
            if ($(this).val().length > 0) {
                $('#emailInputContainer').slideUp();
            } else {
                $('#emailInputContainer').slideDown();
            }

            var inputValue = $(this).val().replace(/\D/g, '');
            if (inputValue.length <= 11) {
                if (inputValue.length === 11) {
                    var phoneNumber = inputValue.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
                    $(this).val(phoneNumber);
                } else {
                    $(this).val(inputValue);
                }
            }
        });

        // Autocomplete para email
        $('#emailInput').on('input', function () {
            if ($(this).val().length > 0) {
                $('#phoneInputContainer').slideUp();
            } else {
                $('#phoneInputContainer').slideDown();
            }

            var inputValue = $(this).val();
            if (inputValue.includes('@') && inputValue.indexOf('@') === inputValue.length - 1) {
                var dropdownHtml = '';
                emailDomains.forEach(function (domain) {
                    dropdownHtml += '<div class="autocomplete-suggestion">' + inputValue + domain + '</div>';
                });
                $('#emailAutocomplete').html(dropdownHtml).fadeIn();
            } else {
                $('#emailAutocomplete').fadeOut();
            }
        });

        $(document).on('click', '.autocomplete-suggestion', function () {
            var selectedEmail = $(this).text();
            $('#emailInput').val(selectedEmail);
            $('#emailAutocomplete').fadeOut();
        });

        $('#loginForm').on('submit', function (e) {
            var phoneValue = $('#phoneInput').val().trim();
            var emailValue = $('#emailInput').val().trim();

            if ((!isValidEmail(emailValue) && emailValue.length > 0) || (!isValidPhone(phoneValue) && phoneValue.length > 0)) {
                e.preventDefault();
                $('#errorMessage').fadeIn();
            } else {
                $('#errorMessage').fadeOut();
                $('#loadingSpinner').fadeIn();
                $('button[type="submit"]').prop('disabled', true);
            }
        });
    }

    // Exibir o toast de erro de login, se houver
    if ($('#loginErrorToast').length > 0) {
        var toastEl = new bootstrap.Toast(document.getElementById('loginErrorToast'));
        toastEl.show();
    }

    // Lógica do menuPrincipal
    if ($('#menuPrincipal').length > 0) {
        $('#historicoButton, #servicoButton').on('click', function (e) {
            $('#loadingSpinner').fadeIn();
            $(this).prop('disabled', true);
        });
    }

    // Lógica para a página de Solicitar Serviço
    if ($('#solicitarServicoPage').length > 0) {
        var servicosSelecionados = JSON.parse(localStorage.getItem('servicosSelecionados')) || [];
        var valorTotal = servicosSelecionados.reduce((total, servico) => total + parseFloat(servico.preco), 0);
        var duracaoTotal = servicosSelecionados.reduce((total, servico) => total + parseInt(servico.duracao), 0); // Nova lógica para duração

        window.adicionarServico = function (id, nome, preco, duracao, element) {
            var index = servicosSelecionados.findIndex(servico => servico.id === id);

            if (index === -1) {
                servicosSelecionados.push({ id, nome, preco, duracao });
                valorTotal += parseFloat(preco);
                duracaoTotal += parseInt(duracao); // Atualiza duração total
                $(element).prop('disabled', true);
            }

            atualizarListaServicosSelecionados();
            localStorage.setItem('servicosSelecionados', JSON.stringify(servicosSelecionados));
            localStorage.setItem('duracaoTotalServicos', duracaoTotal); // Salva duração total
        };

        window.removerServico = function (index, id) {
            valorTotal -= parseFloat(servicosSelecionados[index].preco);
            duracaoTotal -= parseInt(servicosSelecionados[index].duracao); // Remove duração
            servicosSelecionados.splice(index, 1);

            $('#servico-' + id).prop('disabled', false);
            atualizarListaServicosSelecionados();
            localStorage.setItem('servicosSelecionados', JSON.stringify(servicosSelecionados));
            localStorage.setItem('duracaoTotalServicos', duracaoTotal); // Atualiza duração total
        };

        function atualizarListaServicosSelecionados() {
            var lista = $('#servicosSelecionados');
            lista.empty();

            servicosSelecionados.forEach(function (servico, index) {
                lista.append(
                    `<li class="list-group-item d-flex justify-content-between align-items-center">
                        <span>${servico.nome} - R$ ${servico.preco}</span>
                        <button class="btn btn-danger btn-sm" onclick="removerServico(${index}, '${servico.id}')">Remover</button>
                    </li>`
                );
            });

            $('#valorTotal').text(valorTotal.toFixed(2));
        }

        window.confirmarServico = function () {
            if (servicosSelecionados.length === 0) {
                alert('Nenhum serviço selecionado.');
                return;
            }

            $('#loadingSpinner').fadeIn();
            setTimeout(function () {
                $('#loadingSpinner').fadeOut();
                window.location.href = "/Cliente/EscolherBarbeiro";
            }, 2000);
        };

        atualizarListaServicosSelecionados();
    }

    // Lógica para a página de Escolher Barbeiro
    if ($('#escolherBarbeiroPage').length > 0) {
        $('.barbeiro-btn').on('click', function () {
            var barbeiroId = $(this).data('barbeiro-id');
            var duracaoTotal = localStorage.getItem('duracaoTotalServicos');

            $('#calendarioModal').modal('show');
            carregarCalendario(barbeiroId, duracaoTotal);
        });

        function carregarCalendario(barbeiroId, duracaoTotal) {
            const container = document.getElementById('calendario');
            const calendar = new tui.Calendar(container, {
                defaultView: 'week',
                taskView: false,
                scheduleView: ['time'],
                useCreationPopup: true,
                useDetailPopup: true,
                week: {
                    startDayOfWeek: 0, // Começa no Domingo
                    dayNames: ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'], // Dias em português
                    hourStart: 9, // Hora de início
                    hourEnd: 18, // Hora de término
                    timeFormat: 'HH:mm', // Formato de 24 horas
                },
                theme: {
                    'week.timegridSchedule.backgroundColor': '#28a745',  // Cor verde para horários disponíveis
                    'week.timegridSchedule.borderColor': '#28a745',     // Borda verde
                    'week.timegridSchedule.color': '#000',              // Texto preto para melhor contraste
                    'week.scheduleView.headerFontWeight': 'bold'        // Negrito
                }
            });

            // Carregar os horários disponíveis via AJAX
            $.ajax({
                url: '/Cliente/ObterHorariosDisponiveis',
                data: {
                    barbeiroId: barbeiroId,
                    duracaoTotal: duracaoTotal // Duração total dos serviços selecionados
                },
                success: function (data) {
                    var eventos = data.map(function (evento) {
                        var startTime = dayjs(evento).format('DD/MM/YYYY HH:mm');  // Formato BR
                        var endTime = dayjs(evento).add(duracaoTotal, 'minute').format('HH:mm');

                        return {
                            id: evento.id,
                            calendarId: '1',
                            title: `Disponível`,
                            category: 'time',
                            start: evento,
                            end: dayjs(evento).add(duracaoTotal, 'minute').toISOString(),  // Adiciona duração ao fim
                            isReadOnly: true,
                            bgColor: '#28a745',        // Cor de fundo verde
                            borderColor: '#28a745',    // Cor da borda verde
                            color: '#000',             // Texto preto para contraste
                            customStyle: { fontWeight: 'bold' }  // Negrito no evento
                        };
                    });
                    calendar.createSchedules(eventos);  // Cria os eventos no calendário
                },
                error: function () {
                    alert('Erro ao carregar os horários.');
                }
            });

            // Limpa o calendário quando o modal for fechado
            $('#calendarioModal').on('hidden.bs.modal', function () {
                calendar.clear(); // Limpa o calendário para evitar duplicação
            });
        }
    }
});
