import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
// import NetworksView from '@/views/NetworksView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue'),
      meta: {
        title: 'TrojoNetworks',
      },
    },
    {
      path: '/about',
      name: 'about',
      component: () => import('@/views/AboutView.vue'),
      meta: {
        title: 'TrojoNetworks | About',
      },
    },
    {
      path: '/account',
      name: 'account',
      component: () => import('@/views/account/ManageAccountView.vue'),
      meta: {
        requiresAuth: true,
        title: 'Your Account',
      },
      children: [
        {
          path: '',
          name: 'manage-account-home',
          component: () => import('@/components/tabs/account/AccountHomeTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Dashboard',
          },
        },
        {
          path: 'proxies',
          name: 'manage-account-proxies',
          component: () => import('@/components/tabs/account/UserProxyTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Proxies',
          },
        },
        {
          path: 'proxies/:userProxyId/edit',
          name: 'manage-account-edit-proxy',
          component: () => import('@/components/tabs/account/EditUserProxyTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit User Proxy',
          },
        },
        {
          path: 'files',
          name: 'manage-account-files',
          component: () => import('@/components/tabs/usercontent/FilesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Files',
          },
        },
        {
          path: 'blogs',
          name: 'manage-account-blogs',
          component: () => import('@/components/tabs/usercontent/BlogsTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Blogs',
          },
        },
        {
          path: 'networks/:networkId/blogs/:blogId/edit',
          name: 'manage-account-blogs-edit',
          component: () => import('@/components/tabs/usercontent/EditBlogTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Blog',
          },
        },
        {
          path: 'configurations',
          name: 'manage-account-configurations',
          component: () => import('@/components/tabs/usercontent/ConfigurationsTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Configurations',
          },
        },
        {
          path: 'networks/:networkId/configurations/:configurationId/edit',
          name: 'manage-account-configurations-edit',
          component: () => import('@/components/tabs/usercontent/EditConfigurationTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Configuration',
          },
        },
        {
          path: 'custom-pages',
          name: 'manage-account-custom-pages',
          component: () => import('@/components/tabs/usercontent/CustomPagesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'User Custom Pages',
          },
        },
        {
          path: 'networks/:networkId/custom-pages/:customPageId/edit',
          name: 'manage-account-custom-pages-edit',
          component: () => import('@/components/tabs/usercontent/EditCustomPageTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Custom Page',
          },
        },
      ],
    },
    {
      path: '/networks',
      name: 'networks',
      component: () => import('@/views/NetworksView.vue'),
      meta: {
        title: 'Networks',
      },
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('@/views/AdminView.vue'),
      meta: {
        requiresAdmin: true,
        title: 'Networks',
      },
      children: [
        {
          path: '',
          name: 'admin-home',
          component: () => import('@/components/tabs/admin/HomeTab.vue'),
          meta: {
            requiresAdmin: true,
            title: 'Dashboard',
          },
        },
        {
          path: 'networks',
          name: 'admin-networks',
          component: () => import('@/components/tabs/admin/NetworksTab.vue'),
          meta: {
            requiresAdmin: true,
            title: 'Networks',
          },
        },
        {
          path: 'users',
          name: 'admin-users',
          component: () => import('@/components/tabs/admin/UsersTab.vue'),
          meta: {
            requiresAdmin: true,
            title: 'Users',
          },
        },
      ],
    },
    {
      path: '/networks/create',
      name: 'create-network',
      component: () => import('@/views/networks/CreateNetworkView.vue'),
      meta: {
        title: 'Create new network',
      },
    },
    {
      path: '/networks/:networkId',
      name: 'info-network',
      component: () => import('@/views/networks/InfoNetworkView.vue'),
      meta: {
        title: 'Network details',
      },
    },
    {
      path: '/networks/:networkId/manage',
      name: 'manage-network',
      component: () => import('@/views/networks/ManageNetworkView.vue'),
      meta: {
        title: 'Manage network',
      },
      children: [
        {
          path: '',
          name: 'manage-network-home',
          component: () => import('@/components/tabs/network/HomeTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Dashboard',
          },
        },
        {
          path: 'edit',
          name: 'manage-network-edit',
          component: () => import('@/components/tabs/network/EditNetworkTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Details',
          },
        },
        {
          path: 'users',
          name: 'manage-network-users',
          component: () => import('@/components/tabs/network/UsersTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Users',
          },
        },
        {
          path: 'roles',
          name: 'manage-network-roles',
          component: () => import('@/components/tabs/network/RolesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Roles',
          },
        },
        {
          path: 'access',
          name: 'manage-network-access',
          component: () => import('@/components/tabs/network/AccessesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Access',
          },
        },
        {
          path: 'files',
          name: 'manage-network-files',
          component: () => import('@/components/tabs/usercontent/FilesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Files',
          },
        },
        {
          path: 'custom-pages',
          name: 'manage-network-custom-pages',
          component: () => import('@/components/tabs/usercontent/CustomPagesTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Custom Pages',
          },
        },
        {
          path: 'blogs',
          name: 'manage-network-blogs',
          component: () => import('@/components/tabs/usercontent/BlogsTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Blogs',
          },
        },
        {
          path: 'blogs/:blogId/edit',
          name: 'manage-network-edit-blog',
          component: () => import('@/components/tabs/usercontent/EditBlogTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Blog',
          },
        },
        {
          path: 'configurations',
          name: 'manage-network-configurations',
          component: () => import('@/components/tabs/usercontent/ConfigurationsTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Configurations',
          },
        },
        {
          path: 'configurations/:configurationId/edit',
          name: 'manage-network-edit-configuration',
          component: () => import('@/components/tabs/usercontent/EditConfigurationTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Configuration',
          },
        },
        {
          path: 'custom-pages/:customPageId/edit',
          name: 'manage-network-edit-custom-page',
          component: () => import('@/components/tabs/usercontent/EditCustomPageTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Custom Page',
          },
        },
        {
          path: 'custom-pages/:customPageId/blocks/:pageBlockId/edit',
          name: 'manage-network-edit-page-block',
          component: () => import('@/components/tabs/usercontent/EditPageBlockTab.vue'),
          meta: {
            requiresAuth: true,
            title: 'Edit Page Block',
          },
        },
      ],
    },
    {
      path: '/networks/:networkId/join',
      name: 'join-network',
      component: () => import('@/views/networks/JoinNetworkView.vue'),
      meta: {
        title: 'Join this network',
      },
    },
    {
      path: '/networks/:networkId/login',
      name: 'login-network',
      component: () => import('@/views/networks/LoginNetworkView.vue'),
      meta: {
        showNavbar: false,
        requiresAuth: false,
        title: 'Sign In',
      },
    },
    {
      path: '/networks/:networkId/signup',
      name: 'signup-network',
      component: () => import('@/views/networks/SignupNetworkView.vue'),
      meta: {
        showNavbar: false,
        requiresAuth: false,
        title: 'Create Account',
      },
    },
    {
      path: '/networks/:networkId/complete-access',
      name: 'complete-access-network',
      component: () => import('@/views/networks/CompleteAccessView.vue'),
      meta: {
        showNavbar: false,
        requiresAuth: false,
        title: 'Complete Access',
      },
    },
    {
      path: '/contact',
      name: 'contact',
      component: () => import('@/views/ContactView.vue'),
      meta: {
        title: 'TrojoNetworks | Contact',
      },
    },
    {
      path: '/tos',
      name: 'terms-of-service',
      component: () => import('@/views/ToSView.vue'),
      meta: {
        title: 'TrojoNetworks | Terms of Service',
      },
    },
    {
      path: '/401',
      name: 'unauthenticated',
      component: () => import('@/views/UnauthenticatedView.vue'),
      meta: {
        title: 'Unauthenticated',
      },
    },
    {
      path: '/403',
      name: 'unauthorized',
      component: () => import('@/views/UnauthorizedView.vue'),
      meta: {
        title: 'Unauthorized',
      },
    },
    {
      path: '/404',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
      meta: {
        title: 'Page not found',
      },
    },
    {
      path: '/:pathMatch(.*)*',
      // redirect: '/404',
      component: () => import('@/views/NotFoundView.vue'),
    },
  ],
});

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();

  if (to.matched.some((record) => record.meta.requiresAuth)) {
    if (!authStore.isAuthenticated) {
      next({
        path: '/403',
        query: { redirect: btoa(to.fullPath) },
      });
      authStore.setModalOpen(true);
      authStore.setModalMode('login');
      return;
    }
  }

  if (to.matched.some((record) => record.meta.requiresAdmin)) {
    if (!(await authStore.isSuperAdmin())) {
      next({ path: '/404' });
      return;
    }
  }

  next();
});

router.afterEach((to) => {
  if (to.meta.title) {
    document.title = to.meta.title as string;
  }
});

export default router;
