document.addEventListener("keydown", e =>
{
    const activeElement = document.activeElement;
    if (activeElement.tagName === "INPUT" || activeElement.tagName === "TEXTAREA" || activeElement.isContentEditable) {
        return;
    }
    
    const modifiers = [];
    if (e.ctrlKey) {
        modifiers.push("ctrl");
    }
    if (e.altKey) {
        modifiers.push("alt");
    }
    if (e.shiftKey) {
        modifiers.push("shift");
    }
    if (e.metaKey) {
        modifiers.push("meta");
    }
    
    modifiers.push(e.key.toLowerCase());
    const shortcut = modifiers.join("+");
    
    const target = document.querySelector(`[data-shortcut="${shortcut}"]`);
    if (target) {
        e.preventDefault();
        target.focus();
        target.click();
    }
});