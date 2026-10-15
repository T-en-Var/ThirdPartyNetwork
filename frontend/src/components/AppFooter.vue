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
@use '../assets/styles/variables' as *;

.footer {
  margin-top: auto;
  border-top: 1px solid var(--input-border);
  background-color: var(--bg-color);
}
.footer__inner {
  padding: .5rem 1rem;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;

  @media (max-width: $layout-sm) {
    flex-direction: column;
    align-items: flex-start;
  }
}
.footer__brand {
  display: flex;
  align-items: center;
  gap: 1rem;
  color: var(--text-light);
  font-size: 0.85rem;
}
.footer__logo {
  font-weight: 700;
  color: var(--text-color);
}
.footer__copyright {
  white-space: nowrap;
}
.footer__links {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 0.85rem;
  color: var(--text-light);
}
.footer__link {
  font-weight: 500;
}
</style>
