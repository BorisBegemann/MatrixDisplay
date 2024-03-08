// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
var canvas, ctx, flag = false,
    prevX = 0,
    currX = 0,
    prevY = 0,
    currY = 0,
    dot_flag = false;

function init() {
    canvas = document.getElementById('can');
    ctx = canvas.getContext("2d");
    w = canvas.width;
    h = canvas.height;
    
    ctx.fillStyle = "darkblue";
    ctx.fillRect(0, 0, w, h);
    
    canvas.addEventListener("mousemove", function (e) {
        findPosition('move', e)
    }, false);
    canvas.addEventListener("mousedown", function (e) {
        findPosition('down', e)
    }, false);
    canvas.addEventListener("mouseup", function (e) {
        findPosition('up', e)
    }, false);
    canvas.addEventListener("mouseout", function (e) {
        findPosition('out', e)
    }, false);
}

function draw() {
    ctx.beginPath();
    ctx.moveTo(prevX, prevY);
    ctx.lineTo(currX, currY);
    ctx.strokeStyle = "white";
    ctx.lineWidth = 2;
    ctx.stroke();
    ctx.closePath();
}

function erase() {
    ctx.clearRect(0, 0, w, h);
    ctx.fillStyle = "darkblue";
    ctx.fillRect(0, 0, w, h);
}

function save() {
    const data = new FormData();
    data.append('image', canvas.toDataURL());
    const xhr = new XMLHttpRequest();
    xhr.open('POST', '?handler=Image', true);
    xhr.onload = function () {
        console.log(this.responseText);
    };
    xhr.send(data);
}

function findPosition(res, e) {
    if (res === 'down') {
        prevX = currX;
        prevY = currY;
        currX = e.clientX - canvas.offsetLeft;
        currY = e.clientY - canvas.offsetTop;
        flag = true;
        dot_flag = true;
        if (dot_flag) {
            ctx.beginPath();
            ctx.fillStyle = "white";
            ctx.fillRect(currX, currY, 2, 2);
            ctx.closePath();
            dot_flag = false;
        }
    }
    if (res === 'up' || res === "out") {
        flag = false;
    }
    if (res === 'move') {
        if (flag) {
            prevX = currX;
            prevY = currY;
            currX = e.clientX - canvas.offsetLeft;
            currY = e.clientY - canvas.offsetTop;
            draw();
        }
    }
}
