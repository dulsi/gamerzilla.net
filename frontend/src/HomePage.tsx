import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Page } from './Page';
import { getUserList, UserData } from './User';
import { UserList } from './UserList';

export const HomePage = () => {
  const [userlist, setUserList]
    = useState<UserData[] | null>(null);
  const [userListLoading, setUserListLoading] = useState(true);
  useEffect(() => {
    const doGetUserList = async () => {
      if (userListLoading)
      {
        const glist = await getUserList();
        setUserList(glist);
        setUserListLoading(false);
      }
    };
    doGetUserList();
  });
  return (
    <Page title="User List">
      {userlist == null ? (
        <div>
        Loading...
        </div>
      ) : (
        <UserList data={userlist || []} />
      )}
    </Page>
  );
}
