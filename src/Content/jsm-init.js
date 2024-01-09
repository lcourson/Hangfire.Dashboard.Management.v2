function AddScriptTag(url) {
	let link = document.createElement('script');
	link.setAttribute('src', url);
	document.getElementsByTagName('body')[0].appendChild(link);
}
function AddLinkTag(url) {
	let link = document.createElement('link');
	link.setAttribute('rel', 'stylesheet');
	link.setAttribute('href', url);
	document.getElementsByTagName('head')[0].appendChild(link);
}
function AddJSMScriptTags() {
	let jsUrl = $("#hdmConfig").data("hdmjsbundleurl")

	AddScriptTag(jsUrl);

	$('.credit').append('<li>|</li><li>Management ' + $("#hdmConfig").data("version") + "</li>");
}

if (window.attachEvent) {
	window.attachEvent('onload', AddJSMScriptTags);
}
else {
	if (window.onload) {
		let curronload = window.onload;
		let newonload = function (evt) {
			{
				curronload(evt);
				AddJSMScriptTags();
			}
		};
		window.onload = newonload;
	}
	else {
		window.onload = AddJSMScriptTags;
	}
}
