import { useEffect, useState } from 'react';
import { Page } from './Page';
import { getUserList, UserData } from './User';
import { UserList } from './UserList';
import { Box, Flex, Spinner } from '@radix-ui/themes';

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
        <Flex justify="center" p="5">
          <Spinner size="3" />
        </Flex>
      ) : (
        <UserList data={userlist} />
      )}
    </Page>
  );
}
