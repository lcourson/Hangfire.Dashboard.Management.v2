(function (hangfire) {

	hangfire.Management = (function () {
		function Management() {
			this._initialize();
		}
		Management.prototype._initialize = function () {
			$(".commands-panel.hide, .commands-options.hide").each(function () {
				$(this).hide().removeClass('hide');
			})

			$("div[id$='_datetimepicker']").each(function () {
				let tdElement = $(this)
				let tdInput = $(this).find('input')

				if (tdInput) {
					let defaultVal = tdInput.val();
					let td = new tempusDominus.TempusDominus(this);

					tdElement.on('change.td', function (tdEvent) {
						//console.log(tdEvent.date.toISOString())
						tdElement.data('date', tdEvent.date.toISOString());
					});

					if (defaultVal) {
						let parsedDate = td.dates.parseInput(new Date(defaultVal));
						td.dates.setValue(parsedDate);
					}
				}
			});

			$('input.time[data-inputmask]').each(function () {
				Inputmask($(this).data('inputmask')).mask(this);
			});

			$('.hdm-management').each(function () {
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

				$(this).on('click', '.hdm-management-input-commands',
					function (e) {
						var $this = $(this);
						var confirmText = $this.data('confirm');

						var id = $this.attr("input-id");
						var type = $this.attr("input-type");
						var send = { id: id, type: type };

						// Get all non-System control values
						$("input.hdm-job-input.hdm-input-checkbox[id^='" + id + "']", container).each(function () {
							//console.log('Reading Checkbox Input: ' + $(this).prop('id') + ' => ' + $(this).is(':checked'));

							if ($(this).is(':checked')) {
								send[$(this).prop('id')] = "on";
							}
						});
						$("input.hdm-job-input.hdm-input-text[id^='" + id + "']", container).each(function () {
							//console.log('Reading Text Input: ' + $(this).prop('id') + ' => ' + $(this).val());
							send[$(this).prop('id')] = $(this).val();
						});
						$("input.hdm-job-input.hdm-input-number[id^='" + id + "']", container).each(function () {
							//console.log('Reading Number Input: ' + $(this).prop('id') + ' => ' + $(this).val());
							send[$(this).prop('id')] = $(this).val();
						});

						$("textarea.hdm-job-input[id^='" + id + "']", container).each(function () {
							//console.log('Reading TextArea Input: ' + $(this).prop('id') + ' => ' + $(this).val());
							send[$(this).prop('id')] = $(this).val();
						});
						$("select.hdm-job-input[id^='" + id + "']", container).each(function () {
							//console.log('Reading Select Input: ' + $(this).prop('id') + ' => ' + $(this).val());
							send[$(this).prop('id')] = $(this).val();
						});

						$(".hdm-job-input.hdm-input-datalist[id^='" + id + "']", container).each(function () {
							//console.log('Reading DataList Input: ' + $(this).prop('id') + ' => ' + $(this).data('selectedvalue'));
							send[$(this).prop('id')] = $(this).data('selectedvalue');
						});

						$("div.hdm-job-input-container.hdm-input-date-container[id^='" + id + "']", container).each(function () {
							//console.log('Reading Date Input: ' + $(this).prop('id') + ' => ' + $(this).data('date'));
							send[$(this).prop('id')] = $(this).data('date');
						});


						if (send.type === 'Enqueue') {
							// Nothing extra to read here
						}
						else if (send.type === 'ScheduleDateTime') {
							let sdtTd = $(".commands-options.ScheduleDateTime .hdm-execution-input[id^='" + id + "']", container)
							if (sdtTd.length > 0) {
								let sdtInput = $($(sdtTd).find('input')[0])
								//console.log('Reading Schedule Date Input: ' + sdtInput.prop('id') + ' => ' + sdtInput.val());
								send[sdtInput.prop('id')] = sdtTd.data('date');
							}
							else {
								Hangfire.Management.alertError(id, 'Unable to find controls for ScheduleDateTime');
								return;
							}
						}
						else if (send.type === 'ScheduleTimeSpan') {
							let sts = $(".commands-options.ScheduleTimeSpan .hdm-execution-input[id^='" + id + "']", container)
							if (sts.length > 0) {
								//console.log('Reading Schedule Timespan Input: ' + sts.prop('id') + ' => ' + sts.val());
								send[sts.prop('id')] = sts.val();
							}
							else {
								Hangfire.Management.alertError(id, 'Unable to find controls for ScheduleTimeSpan');
								return;
							}

							// This adds a special parameter for when and execute button's drop down is used to specify the schedule.
							if ($this.data('schedule')) {
								//console.log('Overriding Input with Predefined Option: ' + sts.prop('id') + ' => ' + $this.data("schedule"));
								send[sts.prop('id')] = $this.data("schedule");
							}
						}
						else if (send.type === 'CronExpression') {
							let cronExpressionInput = $(".commands-options.CronExpression .hdm-execution-input-exp[id^='" + id + "']", container)
							if (cronExpressionInput.length === 1) {
								let val = cronExpressionInput.val();
								//console.log('Reading Cron Expression Input: ' + cronExpressionInput.prop('id') + ' => ' + val);
								send[cronExpressionInput.prop('id')] = val;
							}
							else {
								Hangfire.Management.alertError(id, 'Unable to find control for Cron Expression Input');
								return;
							}

							// This adds a special parameter for when and execute button's drop down is used to specify the schedule.
							if ($this.data('schedule')) {
								//console.log('Overriding Input with Predefined Option: ' + cronExpressionInput.prop('id') + ' => ' + $this.data("schedule"));
								send[cronExpressionInput.prop('id')] = $this.data("schedule");
							}

							let cronNameInput = $(".commands-options.CronExpression .hdm-execution-input-name[id^='" + id + "']", container)
							if (cronNameInput.length === 1) {
								//console.log('Reading Cron Name Input: ' + cronNameInput.prop('id') + ' => ' + cronNameInput.val());
								send[cronNameInput.prop('id')] = cronNameInput.val();
							}
						}
						else {
							throw 'Unknown Execution Type'
						}

						//console.log('form data: ', send);
						$('#' + id + '_success, #' + id + '_error').empty();
						if (!confirmText || confirm(confirmText)) {
							$this.prop('disabled');
							$this.button('loading');

							$.post($this.data('url'), send, function (data) {
								let taskType = "An Immediate";
								if (send.type === "ScheduleDateTime") { taskType = "A Scheduled"; }
								else if (send.type === "ScheduleTimeSpan") { taskType = "A Delayed"; }
								else if (send.type === "CronExpression") { taskType = "A Recurring"; }
								Hangfire.Management.alertSuccess(id, taskType + " Execution Task has been created. <a href=\"" + data.jobLink + "\">View Job</a>");
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


//console.log('Hangfire Dashboard Management Bundle Starting');
new Hangfire.Management();
