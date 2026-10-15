<template>
  <footer class="footer">
    <div class="footer__inner">
      <div class="footer__brand">
        <span class="footer__logo">TrojoNetworks</span>
        <span class="footer__copyright"
          >&copy; {{ new Date().getFullYear() }} All rights reserved.</span
        >
      </div>
      <div class="footer__links">
        <span>Usefull links -></span>
        <router-link
          to="/contact"
          class="footer__link link"
          >Contact</router-link
        >
        <router-link
          to="/tos"
          class="footer__link link"
          >Terms of Service</router-link
        >
      </div>
    </div>
  </footer>
</template>

<style scoped lang="scss">
.footer {
}
.footer__inner {
  padding: .5rem 1rem;
}
.footer__brand {
  display: flex;
  gap: 1rem;
}
.footer__logo {
}
.footer__copyright {
}
.footer__links {
  display: flex;
  gap: 1rem;
}
.footer__link {
}</style>
