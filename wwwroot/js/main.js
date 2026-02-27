/**
 * Eğitim Platformu - Ana JavaScript
 * Modallar, navigasyon, etkileşimler
 */

document.addEventListener('DOMContentLoaded', function() {
  // Modal aç/kapa
  const modalTriggers = document.querySelectorAll('[data-modal]');
  const modalOverlays = document.querySelectorAll('.modal-overlay');

  modalTriggers.forEach(function(trigger) {
    trigger.addEventListener('click', function(e) {
      e.preventDefault();
      const targetId = this.getAttribute('data-modal');
      const modal = document.getElementById(targetId);
      if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
      }
    });
  });

  modalOverlays.forEach(function(overlay) {
    overlay.addEventListener('click', function(e) {
      if (e.target === overlay) {
        overlay.classList.remove('active');
        document.body.style.overflow = '';
      }
    });

    const closeBtn = overlay.querySelector('.modal-close');
    if (closeBtn) {
      closeBtn.addEventListener('click', function() {
        overlay.classList.remove('active');
        document.body.style.overflow = '';
      });
    }
  });

  // Mobil menü
  const menuToggle = document.querySelector('.menu-toggle');
  const nav = document.querySelector('.nav');

  if (menuToggle && nav) {
    menuToggle.addEventListener('click', function() {
      nav.classList.toggle('open');
      const icon = menuToggle.textContent;
      menuToggle.textContent = icon === '☰' ? '✕' : '☰';
    });
  }

  // Kategori pill tıklama
  document.querySelectorAll('.category-pill').forEach(function(pill) {
    pill.addEventListener('click', function() {
      document.querySelectorAll('.category-pill').forEach(function(p) { p.classList.remove('active'); });
      this.classList.add('active');
    });
  });

  // Tab geçişi
  document.querySelectorAll('.tab').forEach(function(tab) {
    tab.addEventListener('click', function() {
      const parent = this.closest('.tabs') || this.parentElement;
      if (parent) {
        parent.querySelectorAll('.tab').forEach(function(t) { t.classList.remove('active'); });
        this.classList.add('active');
      }
    });
  });

  // Dropdown (mobilde tıklama ile)
  document.querySelectorAll('.dropdown-toggle').forEach(function(toggle) {
    toggle.addEventListener('click', function(e) {
      if (window.innerWidth <= 768) {
        e.preventDefault();
        const dropdown = this.closest('.dropdown');
        if (dropdown) dropdown.classList.toggle('open');
      }
    });
  });

  // Form validasyonu (basit)
  document.querySelectorAll('form[data-validate]').forEach(function(form) {
    form.addEventListener('submit', function(e) {
      let valid = true;
      form.querySelectorAll('[required]').forEach(function(input) {
        if (!input.value.trim()) {
          valid = false;
          input.classList.add('error');
        } else {
          input.classList.remove('error');
        }
      });
      if (!valid) e.preventDefault();
    });
  });

  // Sepet sayacı: Sunucu (session) sepet sayısını Layout'ta CartCount component render ediyor.
  // Sayfa yüklenirken localStorage ile üzerine yazmayalım; aksi halde sayı kaybolur.
  function updateCartCountFromStorage() {
    const countEl = document.getElementById('cart-count');
    if (countEl) {
      try {
        const cart = JSON.parse(localStorage.getItem('cart') || '[]');
        countEl.textContent = cart.length;
        countEl.style.display = cart.length ? 'inline-flex' : 'none';
      } catch (_) {
        countEl.style.display = 'none';
      }
    }
  }

  // Sepete ekle (sadece data-add-to-cart kullanan butonlar için; ana akış form POST ile sunucuya gider)
  document.querySelectorAll('[data-add-to-cart]').forEach(function(btn) {
    btn.addEventListener('click', function(e) {
      e.preventDefault();
      const id = this.getAttribute('data-add-to-cart');
      const title = this.getAttribute('data-course-title') || 'Eğitim';
      const price = this.getAttribute('data-course-price') || '0';
      try {
        let cart = JSON.parse(localStorage.getItem('cart') || '[]');
        if (!cart.find(function(item) { return item.id === id; })) {
          cart.push({ id: id, title: title, price: price });
          localStorage.setItem('cart', JSON.stringify(cart));
          updateCartCountFromStorage();
          if (typeof showToast === 'function') showToast('Sepete eklendi');
          else alert('Sepete eklendi!');
        }
      } catch (_) {}
    });
  });

  // ESC ile modal kapat
  document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
      document.querySelectorAll('.modal-overlay.active').forEach(function(m) {
        m.classList.remove('active');
        document.body.style.overflow = '';
      });
    }
  });

  // Hero slider (anasayfa) – sonsuz döngü + swipe/drag + dots + auto (slayt sayısı dinamik)
  const heroSection = document.querySelector('.hero');
  if (heroSection) {
    const heroSlider = heroSection.querySelector('.hero-slider');
    const heroTrack = heroSection.querySelector('.hero-track');
    var slides = heroTrack ? heroTrack.querySelectorAll('.hero-slide') : [];
    const dots = heroSection.querySelectorAll('.hero-dot');
    var totalSlides = slides.length;
    var totalPhysical = totalSlides + 2;
    var currentSlide = 0;
    var physicalIndex = 1;
    var startX = 0;
    var currentX = 0;
    var isDragging = false;
    var hasMoved = false;
    var slideWidthPx = 0;
    var gapPx = 32; // 2rem slayt arası boşluk (mobilde karışmayı önler)
    var stepPx = 0; // slideWidthPx + gapPx
    var SWIPE_THRESHOLD = 50;
    var justDragged = false;
    var AUTO_SLIDE_MS = 6000;
    var autoSlideTimerId = null;

    function clearAutoSlideTimer() {
      if (autoSlideTimerId) {
        clearTimeout(autoSlideTimerId);
        autoSlideTimerId = null;
      }
    }

    function startAutoSlideTimer() {
      clearAutoSlideTimer();
      autoSlideTimerId = setTimeout(function() {
        autoSlideTimerId = null;
        goToNext();
        startAutoSlideTimer();
      }, AUTO_SLIDE_MS);
    }

    if (heroTrack && totalSlides >= 1) {
      var lastClone = slides[totalSlides - 1].cloneNode(true);
      var firstClone = slides[0].cloneNode(true);
      lastClone.setAttribute('aria-hidden', 'true');
      firstClone.setAttribute('aria-hidden', 'true');
      heroTrack.insertBefore(lastClone, slides[0]);
      heroTrack.appendChild(firstClone);
      slides = heroTrack.querySelectorAll('.hero-slide');
    }

    function measureAndLayout() {
      if (!heroSlider || !heroTrack || !slides.length) return;
      slideWidthPx = heroSlider.clientWidth;
      stepPx = slideWidthPx + gapPx;
      var trackWidth = (slideWidthPx * totalPhysical) + (gapPx * (totalPhysical - 1));
      heroTrack.style.width = trackWidth + 'px';
      for (var s = 0; s < slides.length; s++) {
        slides[s].style.width = slideWidthPx + 'px';
        slides[s].style.minWidth = slideWidthPx + 'px';
        slides[s].style.maxWidth = slideWidthPx + 'px';
      }
    }

    function setTrackTransform(offsetPx) {
      if (!heroTrack || stepPx <= 0) return;
      heroTrack.style.transform = 'translateX(' + (-physicalIndex * stepPx + (offsetPx || 0)) + 'px)';
    }

    function updateDots() {
      for (var j = 0; j < dots.length; j++) {
        dots[j].classList.toggle('active', j === currentSlide);
        dots[j].setAttribute('aria-current', j === currentSlide ? 'true' : null);
      }
    }

    function snapAfterClone() {
      heroTrack.style.transition = 'none';
      heroTrack.style.transform = 'translateX(-' + (physicalIndex * stepPx) + 'px)';
      void heroTrack.offsetHeight;
      heroTrack.style.transition = '';
      updateDots();
    }

    function goToNext() {
      if (!heroTrack || stepPx <= 0) return;
      heroTrack.style.transition = '';
      physicalIndex++;
      heroTrack.style.transform = 'translateX(-' + (physicalIndex * stepPx) + 'px)';
      if (physicalIndex === totalPhysical - 1) {
        currentSlide = 0;
        updateDots();
        var onNextEnd = function() {
          heroTrack.removeEventListener('transitionend', onNextEnd);
          physicalIndex = 1;
          snapAfterClone();
        };
        heroTrack.addEventListener('transitionend', onNextEnd);
      } else {
        currentSlide = physicalIndex - 1;
        updateDots();
      }
    }

    function goToPrev() {
      if (!heroTrack || stepPx <= 0) return;
      heroTrack.style.transition = '';
      physicalIndex--;
      heroTrack.style.transform = 'translateX(-' + (physicalIndex * stepPx) + 'px)';
      if (physicalIndex === 0) {
        currentSlide = totalSlides - 1;
        updateDots();
        var onPrevEnd = function() {
          heroTrack.removeEventListener('transitionend', onPrevEnd);
          physicalIndex = totalSlides;
          snapAfterClone();
        };
        heroTrack.addEventListener('transitionend', onPrevEnd);
      } else {
        currentSlide = physicalIndex - 1;
        updateDots();
      }
    }

    function goToSlide(logicalIndex) {
      if (!heroTrack || stepPx <= 0) return;
      logicalIndex = ((logicalIndex % totalSlides) + totalSlides) % totalSlides;
      currentSlide = logicalIndex;
      physicalIndex = logicalIndex + 1;
      heroTrack.style.transition = '';
      heroTrack.style.transform = 'translateX(-' + (physicalIndex * stepPx) + 'px)';
      void heroTrack.offsetHeight;
      heroTrack.style.transition = '';
      updateDots();
    }

    function onPointerStart(e) {
      if (!heroTrack || !heroSlider) return;
      clearAutoSlideTimer(); // Etkileşim sırasında otomatik geçişi durdur
      var x = e.touches ? e.touches[0].clientX : e.clientX;
      isDragging = true;
      hasMoved = false;
      startX = x;
      currentX = x;
      slideWidthPx = heroSlider.clientWidth;
      stepPx = slideWidthPx + gapPx;
      heroSlider.classList.add('dragging');
    }

    function onPointerMove(e) {
      if (!isDragging || !heroTrack || stepPx <= 0) return;
      var x = e.touches ? e.touches[0].clientX : e.clientX;
      var delta = x - startX;
      if (Math.abs(delta) > 8) hasMoved = true;
      var maxDrag = stepPx * 0.4;
      if (delta > maxDrag) delta = maxDrag;
      if (delta < -maxDrag) delta = -maxDrag;
      setTrackTransform(delta);
      currentX = x;
    }

    function onPointerEnd(e) {
      if (!heroTrack || !heroSlider) return;
      if (!isDragging) return;
      heroSlider.classList.remove('dragging');
      isDragging = false;
      var delta = currentX - startX;
      if (hasMoved && Math.abs(delta) > SWIPE_THRESHOLD) {
        justDragged = true;
        setTimeout(function() { justDragged = false; }, 250);
        if (delta > 0) goToPrev();
        else goToNext();
      } else {
        setTrackTransform(0);
      }
      startAutoSlideTimer(); // Etkileşim bitti, süre sıfırdan başlasın
    }

    if (heroTrack && heroSlider && slides.length > 0 && slides.length === totalPhysical) {
      measureAndLayout();
      setTrackTransform(0);
      requestAnimationFrame(function() {
        measureAndLayout();
        setTrackTransform(0);
      });
      heroSlider.addEventListener('touchstart', onPointerStart, { passive: true });
      heroSlider.addEventListener('touchmove', onPointerMove, { passive: true });
      heroSlider.addEventListener('touchend', onPointerEnd, { passive: true });
      heroSlider.addEventListener('touchcancel', onPointerEnd, { passive: true });
      heroSlider.addEventListener('mousedown', onPointerStart);
      document.addEventListener('mousemove', onPointerMove);
      document.addEventListener('mouseup', onPointerEnd);
      heroSlider.addEventListener('mouseleave', onPointerEnd);
    }

    for (var k = 0; k < dots.length; k++) {
      (function(idx) {
        dots[idx].addEventListener('click', function() {
          goToSlide(idx);
          startAutoSlideTimer(); // Dot tıklanınca süre sıfırdan başlar
        });
      })(k);
    }

    window.addEventListener('resize', function() {
      measureAndLayout();
      if (!isDragging && heroTrack && stepPx > 0) {
        heroTrack.style.transform = 'translateX(-' + (physicalIndex * stepPx) + 'px)';
      }
    });

    if (heroSlider) {
      heroSlider.addEventListener('click', function(e) {
        if (justDragged && (e.target.closest('a') || e.target.closest('button'))) {
          e.preventDefault();
        }
      }, true);
      heroSlider.addEventListener('dragstart', function(e) {
        if (e.target.tagName === 'IMG' || e.target.closest('.hero-image')) e.preventDefault();
      });
    }

    startAutoSlideTimer();
  }

  // Scroll ile görünür olunca animasyon (animate-on-scroll)
  var animated = document.querySelectorAll('.animate-on-scroll');
  if (animated.length && 'IntersectionObserver' in window) {
    var observer = new IntersectionObserver(function(entries) {
      entries.forEach(function(entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('in-view');
        }
      });
    }, { rootMargin: '0px 0px -60px 0px', threshold: 0.1 });
    animated.forEach(function(el) { observer.observe(el); });
  }
});

// Toast bildirimi (opsiyonel)
function showToast(message) {
  let toast = document.getElementById('toast');
  if (!toast) {
    toast = document.createElement('div');
    toast.id = 'toast';
    toast.style.cssText = 'position:fixed;bottom:24px;left:50%;transform:translateX(-50%);background:#1e293b;color:#fff;padding:12px 24px;border-radius:8px;font-size:0.95rem;z-index:9999;opacity:0;transition:opacity 0.3s;';
    document.body.appendChild(toast);
  }
  toast.textContent = message;
  toast.style.opacity = '1';
  setTimeout(function() { toast.style.opacity = '0'; }, 2500);
}
