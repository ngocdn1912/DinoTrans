window.myJavaScriptFunction = function () {
    document.getElementById('my-dialog').showModal();
}

window.OpenDialog = function (ModalId) {
    document.getElementById(ModalId).showModal();
}

window.OpenDialog1 = function (ModalId) {
    document.getElementById(ModalId).style.display = "block";
}

window.CloseDialog = function (ModalId) {
    document.getElementById(ModalId).close();
}

window.NotOpenDialog = function () {
    const elem = document.getElementById('my-dialog');
    elem.open = false;
}

window.ViewPictures = function (Id) {
    document.getElementById(`dialog-ViewPictures-${Id}`).showModal();
}

window.closeModalViewPictures = function (Id) {
    document.getElementById(`dialog-ViewPictures-${Id}`).close();
}

window.closeModal = function () {
    document.getElementById('my-dialog').close();
}

window.updateCountdown = function (elementId, text) {
    const element = document.getElementById(elementId);
    if (element) {
        element.innerText = text;
    }
};