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
      meta: {
        title: 'TrojoNetworks'
      }
    },
    {
      path: '/about',
      name: 'about',
      component: AboutView,
      meta: {
        title: 'TrojoNetworks | About'
      },
    },
    {
      path: '/account',
      name: 'account',
      component: () => import('@/views/account/ManageAccountView.vue'),
      meta: {
        requiresAuth: true,
        title: 'Your Account'
      },
      children: [
        {
          path: '',
          name: 'manage-account-home',
          component: () => import('@/components/tabs/account/AccountHomeTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Dashboard'
          },
        },
        {
          path: 'proxies',
          name: 'manage-account-proxies',
          component: () => import('@/components/tabs/account/UserProxyTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Proxies'
          },
        },
        {
          path: 'proxies/:userProxyId/edit',
          name: 'manage-account-edit-proxy',
          component: () => import('@/components/tabs/account/EditUserProxyTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit User Proxy'
          },
        },
        {
          path: 'files',
          name: 'manage-account-files',
          component: () => import('@/components/tabs/account/FilesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Files'
          },
        },
      ]
    },
    {
      path: '/networks',
      name: 'networks',
      component: () => import('@/views/NetworksView.vue'),
      meta: {
        title: 'Networks'
      },
    },
    {
      path: '/networks/create',
      name: 'create-network',
      component: () => import('@/views/networks/CreateNetworkView.vue'),
      meta: {
        title: 'Create new network'
      },
    },
    {
      path: '/networks/:networkId',
      name: 'info-network',
      component: () => import('@/views/networks/InfoNetworkView.vue'),
      meta: {
        title: 'Network details'
      },
    },
    {
      path: '/networks/:networkId/manage',
      name: 'manage-network',
      component: () => import('@/views/networks/ManageNetworkView.vue'),
      meta: {
        title: 'Manage network'
      },
      children: [
        {
          path: '',
          name: 'manage-network-home',
          component: () => import('@/components/tabs/network/NetworkDasboard.vue'),
          meta: {
            requiresAuth: true,
            title: 'Dashboard'
          },
        },
        {
          path: 'edit',
          name: 'manage-network-edit',
          component: () => import('@/components/tabs/network/EditNetworkTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Details'
          },
        },
        {
          path: 'users',
          name: 'manage-network-users',
          component: () => import('@/components/tabs/network/UsersTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Users'
          },
        },
        {
          path: 'roles',
          name: 'manage-network-roles',
          component: () => import('@/components/tabs/network/RolesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Roles'
          },
        },
        {
          path: 'access',
          name: 'manage-network-access',
          component: () => import('@/components/tabs/network/AccessesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Access'
          },
        },
        {
          path: 'files',
          name: 'manage-network-files',
          component: () => import('@/components/tabs/network/FilesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Files'
          },
        },
        {
          path: 'custom-pages',
          name: 'manage-network-custom-pages',
          component: () => import('@/components/tabs/network/CustomPagesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Custom Pages'
          },
        },
        {
          path: 'custom-pages/:customPageId/edit',
          name: 'manage-network-edit-custom-page',
          component: () => import('@/components/tabs/network/EditCustomPageTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Custom Page'
          },
        },
        {
          path: 'custom-pages/:customPageId/blocks/:pageBlockId/edit',
          name: 'manage-network-edit-page-block',
          component: () => import('@/components/tabs/network/EditPageBlockTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Page Block'
          },
        },
      ]
    },
    {
      path: '/networks/:networkId/join',
      name: 'join-network',
      component: () => import('@/views/networks/JoinNetworkView.vue'),
      meta: {
        title: 'Join this network'
      },
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
      meta: {
        title: 'TrojoNetworks | Terms of Service'
      },
    },
    {
      path: '/401',
      name: 'unauthenticated',
      component: () => import('@/views/UnauthenticatedView.vue'),
      meta: {
        title: 'Unauthenticated'
      },
    },
    {
      path: '/403',
      name: 'unauthorized',
      component: () => import('@/views/UnauthorizedView.vue'),
      meta: {
        title: 'Unauthorized'
      },
    },
    {
      path: '/404',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
      meta: {
        title: 'Page not found'
      },
    },
  ],
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!authStore.isAuthenticated) {
      next({
        path: '/403',
        query: { redirect: btoa(to.fullPath) }
      })
      authStore.setModalOpen(true)
      authStore.setModalMode("login")
      return
    }
  }

  if (to.matched.some(record => record.meta.requiresAdmin)) {
    if (!authStore.isAdmin) {
      next({ path: '/404' })
      return
    }
  }

  next()
})

router.afterEach((to) => {
  if (to.meta.title) {
    document.title = to.meta.title as string
  }
})

export default router;
