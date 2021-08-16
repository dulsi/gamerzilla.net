import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Page } from './Page';
import { getUserList, UserData } from './User';

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
    <Page title="Home">
      {userlist == null ? (
        <div>
        Loading...
        </div>
      ) : userlist.map(user => (
        <p><Link to={`/games/${user.userName}`}>{user.userName}</Link></p>)
      )}
    </Page>
  );
}