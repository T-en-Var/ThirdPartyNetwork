<template>
  <footer class="footer">
    <div class="footer__inner">
      <div class="footer__brand">
        <span class="footer__logo">TrojoNetworks</span>
        <span class="footer__divider">|</span>
        <span class="footer__copyright"
          >&copy; {{ new Date().getFullYear() }} All rights reserved.</span
        >
      </div>
      <div class="footer__links">
        <router-link
          to="/contact"
          class="footer__link"
          >Contact</router-link
        >
        <router-link
          to="/tos"
          class="footer__link"
          >Terms of Service</router-link
        >
      </div>
    </div>
  </footer>
</template>
