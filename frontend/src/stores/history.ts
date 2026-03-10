import type { Blog, Configuration, CustomPage, Network, PageBlock, User, UserProxy } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useHistoryStore = defineStore('history', () => {
  function genericHistory<T extends { id: string }>(theList: T[], theItem: T, max: number) {
    theList = theList.filter((x) => x.id !== theItem.id);
    theList.push(theItem);

    if (theList.length > max) theList.splice(0, theList.length - max);

    return theList;
  }

  // Account
  const userProxies = ref<UserProxy[]>([]);
  const userProxiesHistoryMax = 3;
  const userProxyVisit = (userProxy: UserProxy) => {
    userProxies.value = genericHistory(userProxies.value, userProxy, userProxiesHistoryMax);
    localStorage.setItem('history_userProxies', JSON.stringify(userProxies.value));
  };

  // Admin
  const networks = ref<Network[]>([]);
  const networkHistoryMax = 3;
  const networkVisit = (network: Network) => {
    networks.value = genericHistory(networks.value, network, networkHistoryMax);
    localStorage.setItem('history_networks', JSON.stringify(networks.value));
  };

  const users = ref<User[]>([]);
  const userHistoryMax = 3;
  const userVisit = (user: User) => {
    users.value = genericHistory(users.value, user, userHistoryMax);
    localStorage.setItem('history_users', JSON.stringify(users.value));
  };

  // Network
  const configurations = ref<Configuration[]>([]);
  const configurationHistoryMax = 3;
  const configurationVisit = (config: Configuration) => {
    configurations.value = genericHistory(configurations.value, config, configurationHistoryMax);
    localStorage.setItem('history_configurations', JSON.stringify(configurations.value));
  };

  const blogs = ref<Blog[]>([]);
  const blogHistoryMax = 3;
  const blogVisit = (blog: Blog) => {
    blogs.value = genericHistory(blogs.value, blog, blogHistoryMax);
    localStorage.setItem('history_blogs', JSON.stringify(blogs.value));
  };

  const customPages = ref<CustomPage[]>([]);
  const customPageHistoryMax = 3;
  const customPageVisit = (page: CustomPage) => {
    customPages.value = genericHistory(customPages.value, page, customPageHistoryMax);
    localStorage.setItem('history_customPages', JSON.stringify(customPages.value));
  };

  const pageBlocks = ref<PageBlock[]>([]);
  const pageBlockHistoryMax = 10;
  const pageBlockVisit = (page: PageBlock) => {
    pageBlocks.value = genericHistory(pageBlocks.value, page, pageBlockHistoryMax);
    localStorage.setItem('history_pageBlocks', JSON.stringify(pageBlocks.value));
  };

  function initialize() {
    const storedUserProxies = localStorage.getItem('history_userProxies');
    if (storedUserProxies) userProxies.value = JSON.parse(storedUserProxies);

    const storedBlogs = localStorage.getItem('history_blogs');
    if (storedBlogs) blogs.value = JSON.parse(storedBlogs);

    const storedConfigs = localStorage.getItem('history_configurations');
    if (storedConfigs) configurations.value = JSON.parse(storedConfigs);

    const storedCustomPages = localStorage.getItem('history_customPages');
    if (storedCustomPages) customPages.value = JSON.parse(storedCustomPages);

    const storedPageBlocks = localStorage.getItem('history_pageBlocks');
    if (storedPageBlocks) pageBlocks.value = JSON.parse(storedPageBlocks);

    const storedNetworks = localStorage.getItem('history_networks');
    if (storedNetworks) networks.value = JSON.parse(storedNetworks);

    const storedUsers = localStorage.getItem('history_users');
    if (storedUsers) users.value = JSON.parse(storedUsers);
  }

  function reset() {
    localStorage.removeItem('history_userProxies');
    userProxies.value = []

    localStorage.removeItem('history_blogs');
    blogs.value = []

    localStorage.removeItem('history_configurations');
    configurations.value = []

    localStorage.removeItem('history_customPages');
    customPages.value = []

    localStorage.removeItem('history_pageBlocks');
    pageBlocks.value = []

    localStorage.removeItem('history_networks');
    networks.value = []

    localStorage.removeItem('history_users');
    users.value = []

  }

  return {
    initialize,
    reset,

    userProxies,
    userProxyVisit,

    networks,
    networkVisit,
    users,
    userVisit,

    blogs,
    blogVisit,
    configurations,
    configurationVisit,
    customPages,
    customPageVisit,
    pageBlocks,
    pageBlockVisit,
  };
});
