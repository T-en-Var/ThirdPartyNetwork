import type { CustomPage, PageBlock, UserProxy } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useHistoryStore = defineStore('history', () => {
  function genericHistory<T extends { id: string }>(theList: T[], theItem: T, max: number) {
    theList = theList.filter(x => x.id !== theItem.id);
    theList.push(theItem);

    if (theList.length > max) theList.splice(0, theList.length - max);

    return theList;
  }

  const customPages = ref<CustomPage[]>([]);
  const customPageHistoryMax = 3;
  const customPageVisit = (page: CustomPage) => customPages.value = genericHistory(customPages.value, page, customPageHistoryMax);

  const pageBlocks = ref<PageBlock[]>([]);
  const pageBlockHistoryMax = 10;
  const pageBlockVisit = (page: PageBlock) => pageBlocks.value = genericHistory(pageBlocks.value, page, pageBlockHistoryMax);

  const userProxies = ref<UserProxy[]>([]);
  const userProxiesHistoryMax = 3;
  const userProxyVisit = (userProxy: UserProxy) => {
    console.log("call", userProxy, userProxies.value)
    userProxies.value = genericHistory(userProxies.value, userProxy, userProxiesHistoryMax);
    console.log(userProxies.value)
  }

  return {
    customPageVisit,
    customPages,
    pageBlockVisit,
    pageBlocks,
    userProxies,
    userProxyVisit,
  };
});
