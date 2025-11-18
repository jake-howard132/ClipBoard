export let firstMessage: any = null;
const listeners: ((msg: any) => void)[] = [];

export function getFirstMessage() {
	return firstMessage;
}

export function subscribeToMessage(cb: (msg: any) => void) {
	listeners.push(cb);
	if (firstMessage !== null) cb(firstMessage);
}

if (window.chrome?.webview) {
	window.chrome.webview.addEventListener('message', (e) => {
		if (!firstMessage) {
			firstMessage = e.data;
		}
		listeners.forEach((fn) => fn(e.data));
	});
}
