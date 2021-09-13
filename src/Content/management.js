(function (hangfire) {

    hangfire.Management = (function () {
        function Management() {
            this._initialize();
        }
        Management.prototype._initialize = function () {

            $("div[id$='_datetimepicker']").each(function () {
                $(this).datetimepicker();
            });

            $('input.time').inputmask();

            $('.js-management').each(function () {
                var container = this;

                var showCommandsPanelOptions = function (commandsType) {

                    $(".commands-panel", container).hide();
                    $(".commands-panel." + commandsType, container).show();

                    $(".commands-options", container).hide();
                    $(".commands-options." + commandsType, container).show();
                    //data-commands-type="Enqueue" data-id="@(id)"
                    $(".commandsType." + id).html($("a[data-commands-type='" + commandsType + "']", container).html());
                };

                var $this = $(this);
                var id = $this.data("id");

                $(this).on('click', '.data-list-options .option',
                    function (e) {
                        e.preventDefault();
                        var $this = $(this);
                        var optionValue = $this.data('optionvalue');
                        var optionText = $this.data('optiontext');

                        var optionsId = $this.parents('ul').data('optionsid');
                        var $button = $('#' + optionsId, container);

                        $button.data('selectedvalue', optionValue);
                        $button.find('.input-data-list-text').text(optionText);

                    });
                $(this).on('click', '.commands-type',
                    function (e) {
                        var $this = $(this);
                        var commandsType = $this.data('commands-type');
                        showCommandsPanelOptions(commandsType);
                        e.preventDefault();
                    });

                $(this).on('click', '.js-management-input-CronModal',
                    function (e) {
                        var $this = $(this);
                        var id = $this.attr("input-id");
                        var cron = $("#" + id + "_sys_cron").val();
                        $("#result").val(cron || "* * * * *").data("cronId", id);
                        $('#analysis').click()
                        $('#cronModal').modal("show")
                        e.preventDefault();
                    });

                $("#connExpressionOk").click(function () {

                    var id = $("#result").data("cronId");
                    var cron = $("#result").val();
                    $("#" + id + "_sys_cron").val(cron);
                    $('#cronModal').modal("hide")
                })

                $(this).on('click', '.js-management-input-commands',
                    function (e) {
                        var $this = $(this);
                        var confirmText = $this.data('confirm');

                        var id = $this.attr("input-id");
                        var type = $this.attr("input-type");
                        var send = { id: id, type: type };

                        $("input[id^='" + id + "']", container).each(function () {
                            if ($(this).is('[type=checkbox]')) {
                                if ($(this).is(':checked')) {
                                    send[$(this).attr('id')] = "on";
                                }
                            } else {
                                send[$(this).attr('id')] = $(this).val();
                            }
                        });
                        $("textarea[id^='" + id + "']", container).each(function () { send[$(this).attr('id')] = $(this).val(); });
                        $("select[id^='" + id + "']", container).each(function () { send[$(this).attr('id')] = $(this).val(); });

                        $(".input-control-data-list[id^='" + id + "']", container).each(function () { send[$(this).attr('id')] = $(this).data('selectedvalue'); });

                        $("div[id^='" + id + "']", container).each(function () { send[$(this).attr('id')] = $(this).data('date'); });

                        if ($this.attr('schedule')) {
                            send[id + '_schedule'] = $this.attr("schedule");
                        }

                        $('#' + id + '_success, #' + id + '_error').empty();
                        if (!confirmText || confirm(confirmText)) {
                            $this.prop('disabled');
                            $this.button('loading');

                            $.post($this.data('url'), send, function (data) {
                                Hangfire.Management.alertSuccess(id, "A Task has been created. <a href=\"" + data.jobLink + "\">View Job</a>");
                            }).fail(function (xhr) {
                                var error = 'Unknown Error';

                                try {
                                    error = JSON.parse(xhr.responseText).errorMessage;
                                } catch (e) { /* ignore error */ }

                                Hangfire.Management.alertError(id, error);
                            }).always(function () {
                                $this.removeProp('disabled');
                                $this.button('reset');
                            });
                        }

                        e.preventDefault();
                    });

                $('.input-group *[title], .btn-group *[title]').tooltip('destroy');
                $('.input-group *[title], .btn-group *[title]').tooltip({ container: 'body' });
            });
        };

        Management.alertSuccess = function (id, message) {
            $('#' + id + '_success')
                .html('<div class="alert alert-success"><a class="close" data-dismiss="alert">×</a><strong>Task Created! </strong><span>' +
                    message +
                    '</span></div>');
        }

        Management.alertError = function (id, message) {
            $('#' + id + '_error')
                .html('<div class="alert alert-danger"><a class="close" data-dismiss="alert">×</a><strong>Error! </strong><span>' +
                    message +
                    '</span></div>');
        }

        return Management;

    })();

})(window.Hangfire = window.Hangfire || {});

new Hangfire.Management();