function sendImage(imageName) {
    const data = new FormData();
    data.append('imageName', imageName);
    const xhr = new XMLHttpRequest();
    xhr.open('POST', '?handler=ShowImage', true);
    xhr.onload = function () {
        console.log(this.responseText);
    };
    xhr.send(data);
}