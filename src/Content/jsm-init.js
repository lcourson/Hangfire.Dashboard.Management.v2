// ScriptLoader was found on https://dev.to/timber/wait-for-a-script-to-load-in-javascript-579k
class ScriptLoader {
	constructor(options) {
		const { src, global, protocol = document.location.protocol } = options
		this.src = src
		this.global = global
		this.protocol = protocol
		this.isLoaded = false
	}

	loadScript() {
		return new Promise((resolve, reject) => {
			// Create script element and set attributes
			const script = document.createElement('script')
			script.type = 'text/javascript'
			script.async = true
			script.src = this.src

			// Append the script to the DOM
			const el = document.getElementsByTagName('script')[0]
			el.parentNode.insertBefore(script, el)

			// Resolve the promise once the script is loaded
			script.addEventListener('load', () => {
				this.isLoaded = true
				resolve(script)
			})

			// Catch any errors while loading the script
			script.addEventListener('error', () => {
				reject(new Error(`${this.src} failed to load.`))
			})
		})
	}

	load() {
		return new Promise(async (resolve, reject) => {
			if (!this.isLoaded) {
				try {
					await this.loadScript()
					resolve(window[this.global])
				} catch (e) {
					reject(e)
				}
			} else {
				resolve(window[this.global])
			}
		})
	}
}

function SyncLoadInOrder(urls) {
	// Someone please figure out how to do this in a better way with vanillaJS/jQuery
	if (urls.length > 0) {
		//console.log('Importing ', urls[0]);
		let loader = new ScriptLoader({ src: urls[0] })
		loader.load().then(() => {
			urls.shift();
			SyncLoadInOrder(urls);
		});
	}
}

function AddJSMScriptTags() {
	let assetBaseUrl = $("#hdmConfig").data("assetbaseurl")

	let importUrls = [
		`${assetBaseUrl}/libs/PopperJS/popper_min_js`,
		`${assetBaseUrl}/libs/TempusDominus/js/tempus-dominus_min_js`,
		`${assetBaseUrl}/libs/InputMask/inputmask_min_js`,
		`${assetBaseUrl}/management_js`
	];

	SyncLoadInOrder(importUrls);

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
