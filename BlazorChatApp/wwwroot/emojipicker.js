import { EmojiButton } from 'https://cdn.jsdelivr.net/npm/@joeattardi/emoji-button@4.6.2/+esm';

document.addEventListener("DOMContentLoaded", () => {
    const button = document.querySelector("#emoji-btn");
    const input = document.querySelector("#messageInput");

    if (!button || !input) return;

    const picker = new EmojiButton();
    picker.on("emoji", emoji => {
        input.value += emoji;
        input.dispatchEvent(new Event('input'));
    });

    button.addEventListener("click", () => picker.togglePicker(button));
});
