Contrary to FILE_SERVICE_SETUP, i am considering adding the minio in the TPN docker compose, instead of its own container.
That is because I want the authentication to be coupled very tightly.
See ## Access below

Please analyse if this is a good idea, or if it is better to keep them separate.

## Access

File state can be:
- Soft deleted
- Private
- Public
- Draft

Manage access:
- Grant to or revoke access from:
  - Network
  - Network where users have this/these role/roles
  - Network where users have this/these permission/permissions
  - User
- Accesses to be granted/revoked:
  - View
  - Edit
  - Share (/repost, but at it's source is the same content)
  - Comment? (scope for the future maybe)
  - Like? (scope for the future maybe)
- Switch between public/private
- Share with outside source
- Per media (share with network/user/outside source) define what can be done with the file:
  - Public (anyone can share and use)
- If a private file is used in another source, a temporary viewing link is generated which only renders the file once, the loading of the file is after that prohibited with that generated link. So should only be visible in the private media
  - Note: dont use the same link, this could cause user created race conditions. Force new image urls.

## Notes

My idea is to not have public cdn like urls which can forever show the image.
But everytime an image is trying to be loaded in, it generates a one time use viewing link.
If the image is set to public, I guess we can set a permanent viewing link until specified otherwise.

The TPN application will manage the permissions of who can access which files.
With that in mind, we can allow admins to view private files:
- A super admin (admin of main network) can see all files always.
- A regular admin (admin of not main network) can see privated files if they are shared with the network (but, i guess in a hidden state?)
  
Im not done thinking about the regular admin access, because a user could just revoke access of the file to the network and it would appear as "deleted" because it is now gone. Which would make moderating annoying...

My idea is to have the minio cdn, described in FILE_SERVICE_SETUP.md not be directly accessible.
Only throught the TPN will the files be accessible.

It would be cool if the url could be files.example.com/someHash, which would maybe also contain the presigned info for the minio container.
In nginx I think we could just redirect files.example.com/ to api.example.com/cdn/someHash so we can keep all the auth logic inside a single repository.

## Not only to files

Files are easily reused, like in blogs, be it inside the TPN blogs or on an external website.

But I want this system to apply to all UserGeneratedContent on the TPN platform.

For example:

I have a website: exampleweb.com

On this website I show blogs:

- blog A
- blog B
- blog C

Clicking on the blog, requests the blogs.
When they are requested from the specific website, which is registered under the network, as the blogs have been shared to be viewed in the network, the server sends back the data. But if a user were to call the API directly, they would get a 404 or 403.

(This is how it should work for files as well.)

## My questions

### 1.

If user 1 has a file A and shares file A with user 2, user 2 should always be able to view this file.

If user 2 calls files.example.com/fileAhash it retrieves it, assuming the user has is logged in.

My question: if they paste the cdn url files.example.com/fileAhash in their browser, it does not automatically send the bearer token. And in the current setup will be rejected. Q: Should we make a system to allow this if possible? 

### 2.

DATABASE_AND_AUTH_OVERVIEW paragraph 5.5:
These permissions (like ManageFiles) will overrule the access granting altogether. They can then access everything of that content type and edit it within the network they have the permission in, even if that piece of content has been set to view only for that network.

The ReadFiles/ReadBlogs/etc... permissions works differently: If they dont have the permission, they cannot read any files, even if the files have been shared with the network.

It feels weird to have a user upload a piece of content, THEN share it with a network, or several networks, and letting people with the manageContent permission edit that content... as it will affect the same content in other networks as well. Q: right?

Instead we will focus on moderating that content instead of editing it. Users who have the specific manage content permission will not be able to edit the content anymore, but will be able to flag it and/or hide it within the network. Add an arbitrary note to it e.g.: "Please remove x from this blog" or "We do not allow posting NSFW content" 

Q: is this a good approach? What else can we change or allow?

Also, what if a piece of content is only used within a single network?
What if a network would like a policy, that any content created in the network cannot be used in any other network, or by any users who are not or no longer in the network? Should the "moderators" who have the manage content permission be able to edit that content then? I guess it should be like so: A user creates a blog on a website, that website uses TPN. That website has an automatic sharing system when posting a blog. It is shared to the role "Admin" as "is allowed to edit." This could work, but, then if we consider the user who created the piece of content to be the absolute owner, and the user wants to reuse this blog somewhere else, they should be able to... And, if the user is the absolute owner, they could manage the sharing access and remove the role... So then, should there be an option where content is actually network owned? And then it can be shared with the user to edit, but not share further and shared with the admins too.

### 3 

It is easy to check if a user has access to a single piece of content.
But this could become an intense query when filtering through thousands of files.

How can we make that a good and fast query?

My guess, when fetching files (or blogs, or whatever user generated content where the access rules apply), don't read from the files table.
Instead read like so:
(if it were implemented like User -> UserFileAccesses -> Files and Network -> NetworkFileAccesses -> Files)

Where(UserFileAccesses.UserId == x).Include(Files)
or something...

Q: What is a good and efficient way of filtering for access in a big list?


