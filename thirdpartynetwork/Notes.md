For `src\views\networks\ManageNetworkView.vue:196`, it requires an image url.
The FileDTO no longer has an image url.

We only have viewing links.

Time for logic.

Q: How should we access public files?
Always send public links with images which have them?


When a user sets the profile picture, set the new profile picture to public.
Frontend: remind the user that the old picture still exists.



Currently the /authors/{userProxyId}/<contentType> route is defined and shows all content the 
requesting client is allowed to see. This intended to be used by the original author as well to 
see all their own created content. But I'm wondering, do I want to add the AccessGrants, ViewingLinks 
and ModerationFlags? I think I do, but I will see.

Also have to check if the Moderation flags actually hide the content if applicable.