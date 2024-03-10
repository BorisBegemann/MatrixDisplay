function sendSample(index) {
    const data = new FormData();
    data.append('sampleIndex', index);
    const xhr = new XMLHttpRequest();
    xhr.open('POST', '?handler=ShowSample', true);
    xhr.onload = function () {
        console.log(this.responseText);
    };
    xhr.send(data);
}