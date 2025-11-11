import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';

const editor = new Editor({
	element: document.querySelector('#editor'),
	extensions: [StarterKit],
	content: '<p>Hello, <strong>Tiptap</strong>!</p>',
});

const actions = [
	{ command: 'toggleBold', label: 'B', active: 'bold' },
	{ command: 'toggleItalic', label: 'I', active: 'italic' },
	{ command: 'toggleStrike', label: 'S', active: 'strike' },
	{
		command: 'toggleHeading',
		label: 'H1',
		params: { level: 1 },
		active: { heading: { level: 1 } },
	},
	{ command: 'toggleBulletList', label: '• List', active: 'bulletList' },
	{ command: 'toggleOrderedList', label: '1. List', active: 'orderedList' },
	{ command: 'toggleBlockquote', label: '" Quote', active: 'blockquote' },
	{ command: 'toggleCodeBlock', label: '</>', active: 'codeBlock' },
	{ command: 'undo', label: '↶' },
	{ command: 'redo', label: '↷' },
];

const menu = document.querySelector('.menu');

actions.forEach((action) => {
	const button = document.createElement('button');
	button.textContent = action.label;

	button.addEventListener('click', () => {
		const chain = editor.chain().focus();
		action.params
			? chain[action.command](action.params).run()
			: chain[action.command]().run();
		update();
	});

	menu.appendChild(button);
});

function update() {
	const buttons = menu.querySelectorAll('button');
	buttons.forEach((button, idx) => {
		const action = actions[idx];
		const active = action.active;
		let isActive = false;

		if (typeof active === 'string') isActive = editor.isActive(active);
		else if (typeof active === 'object')
			isActive = editor.isActive(
				Object.keys(active)[0],
				Object.values(active)[0]
			);

		button.classList.toggle('is-active', isActive);
	});
}

editor.on('selectionUpdate', update);
editor.on('transaction', update);
update();
