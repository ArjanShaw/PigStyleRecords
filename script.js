﻿
const searchBox = document.getElementById('searchBox');
const gallery = document.getElementById('gallery');

searchBox.addEventListener('input', () => {
    const term = searchBox.value.toLowerCase();
    const cards = Array.from(gallery.children);

    if (!term) {
        // Highlight all if search is empty
        cards.forEach(card => {
            card.classList.add('highlight');
            card.classList.remove('not-highlight');
        });
    } else {
        const matching = [];
        const nonMatching = [];
        cards.forEach(card => {
            const info = card.getAttribute('data-info');
            if (info.includes(term)) {
                card.classList.add('highlight');
                card.classList.remove('not-highlight');
                matching.push(card);
            } else {
                card.classList.remove('highlight');
                card.classList.add('not-highlight');
                nonMatching.push(card);
            }
        });
        // Reshuffle: matching first
        gallery.innerHTML = '';
        matching.forEach(c => gallery.appendChild(c));
        nonMatching.forEach(c => gallery.appendChild(c));
    }
});
