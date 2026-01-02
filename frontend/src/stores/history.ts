import type { Blog, Configuration, CustomPage, PageBlock, UserProxy } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useHistoryStore = defineStore('history', () => {
  function genericHistory<T extends { id: string }>(theList: T[], theItem: T, max: number) {
    theList = theList.filter(x => x.id !== theItem.id);
    theList.push(theItem);

    if (theList.length > max) theList.splice(0, theList.length - max);

    return theList;
  }

  const configurations = ref<Configuration[]>([]);
  const configurationHistoryMax = 3;
  const configurationVisit = (config: Configuration) => configurations.value = genericHistory(configurations.value, config, configurationHistoryMax);

  const blogs = ref<Blog[]>([]);
  const blogHistoryMax = 3;
  const blogVisit = (blog: Blog) => blogs.value = genericHistory(blogs.value, blog, blogHistoryMax);

  const customPages = ref<CustomPage[]>([]);
  const customPageHistoryMax = 3;
  const customPageVisit = (page: CustomPage) => customPages.value = genericHistory(customPages.value, page, customPageHistoryMax);

  const pageBlocks = ref<PageBlock[]>([]);
  const pageBlockHistoryMax = 10;
  const pageBlockVisit = (page: PageBlock) => pageBlocks.value = genericHistory(pageBlocks.value, page, pageBlockHistoryMax);

  const userProxies = ref<UserProxy[]>([]);
  const userProxiesHistoryMax = 3;
  const userProxyVisit = (userProxy: UserProxy) => userProxies.value = genericHistory(userProxies.value, userProxy, userProxiesHistoryMax);

  return {
    customPageVisit,
    customPages,
    pageBlockVisit,
    pageBlocks,
    userProxies,
    userProxyVisit,
    configurations,
    configurationHistoryMax,
    configurationVisit,
    blogs,
    blogHistoryMax,
    blogVisit,
  };
});
