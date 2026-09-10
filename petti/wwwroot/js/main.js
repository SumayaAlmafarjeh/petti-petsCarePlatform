/* ==========================================================================
   PETTI — main.js
   Vanilla ES6+ interactions: nav scroll state, cart badge + toast,
   filter pills, booking modal.
   ========================================================================== */

document.addEventListener('DOMContentLoaded', () => {

    /* ------------------------------------------------------------------------
       Shared state
       ------------------------------------------------------------------------ */
    let cartCount = 0;

    /* ------------------------------------------------------------------------
       1. Sticky nav — glassmorphism backdrop on scroll
       ------------------------------------------------------------------------ */
    const navInner = document.getElementById('navInner');

    const handleNavScroll = () => {
        if (!navInner) return;
        if (window.scrollY > 24) {
            navInner.classList.add('scrolled');
        } else {
            navInner.classList.remove('scrolled');
        }
    };

    handleNavScroll();
    window.addEventListener('scroll', handleNavScroll, { passive: true });

    /* ------------------------------------------------------------------------
       2. Cart badge + Add to Cart toast
       ------------------------------------------------------------------------ */
    const cartBadge = document.getElementById('cartBadge');
    const cartToast = document.getElementById('cartToast');
    const toastTitle = document.getElementById('toastTitle');
    const toastPrice = document.getElementById('toastPrice');
    const toastImg = document.getElementById('toastImg');

    let toastTimer = null;

    const updateCartBadge = () => {
        if (!cartBadge) return;
        cartBadge.textContent = cartCount;
        cartBadge.classList.remove('bump');
        // Force reflow so the animation can retrigger
        void cartBadge.offsetWidth;
        cartBadge.classList.add('bump');
    };

    const showCartToast = (name, price, imgSrc) => {
        if (!cartToast) return;
        toastTitle.textContent = name;
        toastPrice.textContent = price;
        if (toastImg) toastImg.src = imgSrc;

        cartToast.classList.add('show');

        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => {
            cartToast.classList.remove('show');
        }, 3200);
    };

    document.querySelectorAll('.add-cart-btn').forEach((btn) => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const card = btn.closest('.product-card');
            if (!card) return;

            const name = card.dataset.name || 'Item';
            const price = card.dataset.price || '';
            const img = card.querySelector('img')?.getAttribute('src') || '';

            cartCount += 1;
            updateCartBadge();
            showCartToast(name, price, img);
        });
    });

    const toastClose = document.getElementById('toastClose');
    if (toastClose) {
        toastClose.addEventListener('click', () => {
            cartToast.classList.remove('show');
            clearTimeout(toastTimer);
        });
    }

    /* ------------------------------------------------------------------------
       3. Wishlist heart toggle (small delight, no cart impact)
       ------------------------------------------------------------------------ */
    document.querySelectorAll('.wishlist-btn').forEach((btn) => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            btn.classList.toggle('active');
            const icon = btn.querySelector('i');
            if (!icon) return;
            icon.classList.toggle('fa-solid');
            icon.classList.toggle('fa-regular');
        });
    });

    /* ------------------------------------------------------------------------
       4. Quick filter pill toggles (products + services use separate groups)
       ------------------------------------------------------------------------ */
    document.querySelectorAll('[data-filter-group]').forEach((group) => {
        const pills = group.querySelectorAll('.filter-pill');
        pills.forEach((pill) => {
            pill.addEventListener('click', () => {
                pills.forEach((p) => p.classList.remove('active'));
                pill.classList.add('active');

                const filterValue = pill.dataset.filter;
                const targetSelector = group.dataset.filterGroup;
                const items = document.querySelectorAll(`${targetSelector} [data-category]`);

                items.forEach((item) => {
                    const show = filterValue === 'all' || item.dataset.category === filterValue;
                    item.style.display = show ? '' : 'none';
                });
            });
        });
    });

    /* ------------------------------------------------------------------------
       5. Booking modal — service context + time slot selection
       ------------------------------------------------------------------------ */
    const bookingModalEl = document.getElementById('bookingModal');
    const bookingModal = bookingModalEl ? new bootstrap.Modal(bookingModalEl) : null;

    const modalServiceName = document.getElementById('modalServiceName');
    const modalServicePrice = document.getElementById('modalServicePrice');
    const modalServiceIcon = document.getElementById('modalServiceIcon');
    const confirmBookingBtn = document.getElementById('confirmBookingBtn');

    document.querySelectorAll('.book-visit-btn').forEach((btn) => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const card = btn.closest('.service-card');
            if (!card || !bookingModal) return;

            modalServiceName.textContent = card.dataset.serviceName || 'Service';
            modalServicePrice.textContent = card.dataset.servicePrice || '';
            modalServiceIcon.innerHTML = card.querySelector('.service-card-icon')?.innerHTML || '';
            modalServiceIcon.style.background = card.querySelector('.service-card-icon')?.style.background || '';

            // Reset slot selection each time the modal opens for a new service
            document.querySelectorAll('.slot-btn').forEach((s) => s.classList.remove('selected'));
            const firstSlot = document.querySelector('.slot-btn');
            if (firstSlot) firstSlot.classList.add('selected');

            bookingModal.show();
        });
    });

    document.querySelectorAll('.slot-btn').forEach((slot) => {
        slot.addEventListener('click', () => {
            document.querySelectorAll('.slot-btn').forEach((s) => s.classList.remove('selected'));
            slot.classList.add('selected');
        });
    });

    if (confirmBookingBtn) {
        confirmBookingBtn.addEventListener('click', () => {
            const serviceName = modalServiceName ? modalServiceName.textContent : 'your visit';
            bookingModal?.hide();
            showCartToast(`Visit booked: ${serviceName}`, 'Confirmation sent', modalServiceIcon?.querySelector('i') ? '' : '');
            // Reuse the toast purely for confirmation feedback; hide image if none provided
            if (toastImg) toastImg.style.display = 'none';
            setTimeout(() => { if (toastImg) toastImg.style.display = ''; }, 3300);
        });
    }

    /* ------------------------------------------------------------------------
       6. Newsletter form — lightweight inline feedback, no page reload
       ------------------------------------------------------------------------ */
    const newsletterForm = document.getElementById('newsletterForm');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const input = newsletterForm.querySelector('input');
            const btn = newsletterForm.querySelector('button');
            if (!input || !input.value.trim()) return;
            const originalIcon = btn.innerHTML;
            btn.innerHTML = '<i class="fa-solid fa-check"></i>';
            input.value = '';
            setTimeout(() => { btn.innerHTML = originalIcon; }, 2200);
        });
    }


});

// Global Cart Handling
async function refreshCartBadge() {
    try {
        const res = await fetch('/Cart/GetSummary');
        if (res.ok) {
            const data = await res.json();
            const badge = document.getElementById('cartBadge');
            if (badge) badge.textContent = data.itemsCount;
        }
    } catch (err) {
        console.error('Error fetching cart summary', err);
    }
}

function showCartToast(title, price, img) {
    const toast = document.getElementById('cartToast');
    const toastTitle = document.getElementById('toastTitle');
    const toastPrice = document.getElementById('toastPrice');
    const toastImg = document.getElementById('toastImg');

    if (toastTitle) toastTitle.textContent = title;
    if (toastPrice) toastPrice.textContent = `${price} JOD`;
    if (toastImg && img) toastImg.src = img;

    if (toast) {
        toast.classList.add('show');
        setTimeout(() => toast.classList.remove('show'), 3500);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    refreshCartBadge();

    // Attach click event for dynamic add-to-cart buttons
    document.addEventListener('click', async (e) => {
        const btn = e.target.closest('.add-to-cart-btn');
        if (!btn) return;

        e.preventDefault();
        const productId = btn.dataset.id;

        try {
            const res = await fetch(`/Cart/AddToCart?productId=${productId}`, {
                method: 'POST'
            });

            if (res.ok) {
                const data = await res.json();
                if (data.success) {
                    const badge = document.getElementById('cartBadge');
                    if (badge) badge.textContent = data.itemsCount;
                    showCartToast(data.productName, data.price, data.img);
                }
            }
        } catch (err) {
            console.error('Error adding to cart', err);
        }
    });

    document.getElementById('toastClose')?.addEventListener('click', () => {
        document.getElementById('cartToast')?.classList.remove('show');
    });
});