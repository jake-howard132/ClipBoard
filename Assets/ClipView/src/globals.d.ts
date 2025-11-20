declare global {
	interface Window {
		chrome?: {
			webview?: {
				postMessage: (message: any) => void;
				addEventListener: (type: 'message', listener: (e: any) => void) => void;
				removeEventListener: (
					type: 'message',
					listener: (e: any) => void
				) => void;
			};
			webkit?: {};
		};
	}
}

export {};
