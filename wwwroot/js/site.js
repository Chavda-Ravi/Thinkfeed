// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    const carousels = document.querySelectorAll('.blog-image-carousel');

    carousels.forEach(function(carousel) {
        const track = carousel.querySelector('.blog-image-track');
        if (!track) return; // nothing to do

        const slides = carousel.querySelectorAll('.carousel-slide');
        const prev = carousel.querySelector('.carousel-prev');
        const next = carousel.querySelector('.carousel-next');
        let index = 0;
        const total = slides.length;

        if (total <= 1) {
            carousel.classList.add('single');
        }

        function update() {
            track.style.transform = 'translateX(' + (-index * 100) + '%)';
        }

        if (prev) prev.addEventListener('click', function() {
            index = (index - 1 + total) % total;
            update();
        });

        if (next) next.addEventListener('click', function() {
            index = (index + 1) % total;
            update();
        });

        // keyboard support when focused
        carousel.addEventListener('keydown', function(e) {
            if (e.key === 'ArrowLeft' && total > 1) {
                index = (index - 1 + total) % total;
                update();
            } else if (e.key === 'ArrowRight' && total > 1) {
                index = (index + 1) % total;
                update();
            }
        });

        // make carousel focusable for keyboard use
        if (!carousel.hasAttribute('tabindex')) {
            carousel.setAttribute('tabindex', '0');
        }

        // initial render
        update();
    });
});
