// A simple javasctipt test.
document.getElementById("js-test").textContent = new Date().toLocaleString();

var canvas = document.getElementById('dots');
var context = canvas.getContext('2d');

canvas.height = window.innerHeight
canvas.width = window.innerWidth

var canvasWidth = canvas.width;
var canvasHeight = canvas.height;


//canvas.attr({height: canvasHeight, width: canvasWidth});

// Set an array of dot objects.
var dots = [
  { x: 100, y: 100, radius: 1, xMove: '+', yMove: '+' },
  { x: 100, y: 100, radius: 1, xMove: '-', yMove: '+' },
  { x: 100, y: 100, radius: 1, xMove: '+', yMove: '-' },
  { x: 1000, y: 400, radius: 1, xMove: '+', yMove: '-' },
  { x: 1000, y: 400, radius: 1, xMove: '-', yMove: '+' },
  { x: 1000, y: 400, radius: 1, xMove: '-', yMove: '-' }
];

// Notice in the moveDot function we can make dots go faster if we increment
// by more than 1 pixel each time.
var frameLength = 2;

// Draw each dot in the dots array.
for( i = 0; i < dots.length; i++ ) {
  drawDot(dots[i]);
};

setTimeout(function(){
  window.requestAnimationFrame(moveDot);
}, 2500);


function moveDot() {
  context.clearRect(0, 0, canvasWidth, canvasHeight)

  // Iterate over all the dots.
  for( i = 0; i < dots.length; i++ ) {

    if( dots[i].xMove == '+' ) {
      dots[i].x += frameLength;
    } else {
      dots[i].x -= frameLength;
    }
    if( dots[i].yMove == '+' ) {
      dots[i].y += frameLength;
    } else {
      dots[i].y -= frameLength;
    }

    drawDot(dots[i])

    if( (dots[i].x + dots[i].radius) >= canvasWidth ) {
      dots[i].xMove = '-';
    }
    if( (dots[i].x - dots[i].radius) <= 0 ) {
      dots[i].xMove = '+';
    }
    if( (dots[i].y + dots[i].radius) >= canvasHeight ) {
      dots[i].yMove = '-';
    }
    if( (dots[i].y - dots[i].radius) <= 0 ) {
      dots[i].yMove = '+';
    }
  }

  // Draw lines.
  drawLine(dots[0], dots[1]);
  drawLine(dots[1], dots[2]);
  drawLine(dots[2], dots[0]);
  drawLine(dots[3], dots[4]);
  drawLine(dots[4], dots[5]);
  drawLine(dots[5], dots[3]);

  // Render it again
  window.requestAnimationFrame(moveDot);
}

function drawDot(dot) {
  context.beginPath();
  context.arc(dot.x, dot.y, dot.radius, 0, 2 * Math.PI, false);
  context.strokeStyle = '#F03C69';
//   context.strokeWidth = 0.1;
  context.stroke();
}

function drawLine(dot1, dot2) {
  context.beginPath();
  context.moveTo(dot1.x, dot1.y);
  context.lineTo(dot2.x, dot2.y);
  context.strokeStyle = '#F03C69';
  context.stroke();
}
