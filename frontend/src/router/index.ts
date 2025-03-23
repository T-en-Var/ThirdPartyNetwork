import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import { useAuthStore } from '@/stores/auth';
import NotFoundView from '@/views/NotFoundView.vue';
import AboutView from '@/views/AboutView.vue';
import TOSView from '@/views/TOSView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: {},
    },
    {
      path: '/about',
      name: 'about',
      component: AboutView,
      meta: {},
    },
    {
      path: '/tos',
      name: 'Terms of Service',
      component: TOSView,
      meta: {},
    },
    {
      path: '/404',
      name: 'not-found',
      component: NotFoundView,
      meta: {},
    },
    // {
    //   path: '/account',
    //   name: 'account',
    //   component: AccountView,
    //   meta: {
    //     requiresAuth: true,
    //   },
    // },
  ],
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!authStore.isAuthenticated()) {
      next({
        path: '/login',
        query: { redirect: to.fullPath }
      })
      return
    }
  }

  if (to.matched.some(record => record.meta.requiresAdmin)) {
    if (!authStore.isAdmin()) {
      next({ path: '/404' })
      return
    }
  }

  next()
})

export default router;
