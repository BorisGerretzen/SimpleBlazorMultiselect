// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function register(dotNetHelper, container) {
    function handleClick(event) {
        if (!container.contains(event.target)) {
            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }
    }

    document.addEventListener('click', handleClick);
    return {
        dispose: () => {
            document.removeEventListener('click', handleClick);
        }
    };
}
