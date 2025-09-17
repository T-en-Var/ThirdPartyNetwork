import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import { useAuthStore } from '@/stores/auth';
import AboutView from '@/views/AboutView.vue';
import ToSView from '@/views/ToSView.vue';
// import NetworksView from '@/views/NetworksView.vue';

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
      path: '/account',
      name: 'account',
      component: () => import('@/views/account/ManageAccountView.vue'),
      meta: {
        requiresAuth: true,
      },
      children: [
        {
          path: '',
          name: 'manage-account-home',
          component: () => import('@/components/tabs/account/AccountHomeTab.vue'),
          meta: {
            title: 'Dashboard'
          },
        },
        {
          path: 'proxies',
          name: 'manage-account-proxies',
          component: () => import('@/components/tabs/account/UserProxyTab.vue'),
          meta: {
            title: 'User Proxies'
          },
        },
      ]
    },
    {
      path: '/networks',
      name: 'networks',
      component: () => import('@/views/NetworksView.vue'),
      meta: {},
    },
    {
      path: '/networks/create',
      name: 'create-network',
      component: () => import('@/views/networks/CreateNetworkView.vue'),
      meta: {},
    },
    {
      path: '/networks/:networkId',
      name: 'info-network',
      component: () => import('@/views/networks/InfoNetworkView.vue'),
      meta: {},
    },
    {
      path: '/networks/:networkId/manage',
      name: 'manage-network',
      component: () => import('@/views/networks/ManageNetworkView.vue'),
      meta: {},
      children: [
        {
          path: '',
          name: 'manage-network-home',
          component: () => import('@/components/tabs/network/NetworkDasboard.vue'),
          meta: {
            requiresAuth: false,
            title: 'Dashboard'
          },
        },
        {
          path: 'users',
          name: 'manage-network-users',
          component: () => import('@/components/tabs/network/UsersTab.vue'),
          meta: {
            requiresAuth: false,
            title: 'Users'
          },
        },
        {
          path: 'roles',
          name: 'manage-network-roles',
          component: () => import('@/components/tabs/network/RolesTab.vue'),
          meta: {
            requiresAuth: false,
            title: 'Roles'
          },
        },
        {
          path: 'access',
          name: 'manage-network-access',
          component: () => import('@/components/tabs/network/AccessesTab.vue'),
          meta: {
            requiresAuth: false,
            title: 'Complete Access'
          },
        },
        {
          path: 'files',
          name: 'manage-network-files',
          component: () => import('@/components/tabs/network/FilesTab.vue'),
          meta: {
            requiresAuth: false,
            title: 'Complete Access'
          },
        },
      ]
    },
    {
      path: '/networks/:networkId/join',
      name: 'join-network',
      component: () => import('@/views/networks/JoinNetworkView.vue'),
      meta: {},
    },
    {
      path: '/networks/:networkId/login',
      name: 'login-network',
      component: () => import('@/views/networks/LoginNetworkView.vue'),
      meta: {
        showNavbar: false,
        requiresAuth: false,
        title: 'Sign In'
      },
    },
    {
      path: '/networks/:networkId/signup',
      name: 'signup-network',
      component: () => import('@/views/networks/SignupNetworkView.vue'),
      meta: {
        showNavbar: false,
        requiresAuth: false,
        title: 'Create Account'
      },
    },
    {
      path: '/networks/:networkId/complete-access',
      name: 'complete-access-network',
      component: () => import('@/views/networks/CompleteAccessView.vue'),
      meta: {
        showNavbar: false,
        requiresAuth: false,
        title: 'Complete Access'
      },
    },
    {
      path: '/tos',
      name: 'Terms of Service',
      component: ToSView,
      meta: {},
    },
    {
      path: '/401',
      name: 'unauthenticated',
      component: () => import('@/views/UnauthenticatedView.vue'),
      meta: {},
    },
    {
      path: '/403',
      name: 'unauthorized',
      component: () => import('@/views/UnauthorizedView.vue'),
      meta: {},
    },
    {
      path: '/404',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
      meta: {},
    },
  ],
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!authStore.isAuthenticated()) {
      next({
        path: '/403',
        query: { redirect: btoa(to.fullPath) }
      })
      authStore.setModelOpen(true)
      authStore.setModelMode("login")
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
