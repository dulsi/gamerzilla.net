import { FC, useState, useCallback, useEffect, useRef, useMemo, useLayoutEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useWindowVirtualizer } from '@tanstack/react-virtual';
import { Box, TextField, Text, Strong, Flex } from '@radix-ui/themes';
import { Search } from 'lucide-react';
import { UserData, approve, deleteUser, demoteAdmin, promoteToAdmin } from './User';
import { ConfirmDialog } from './ConfirmDialog';
import { UserCard } from './UserCard';
import { useAuth } from './AuthContext';

const useContainerWidth = (ref: React.RefObject<HTMLElement | null>) => {
  const [width, setWidth] = useState(0);
  useEffect(() => {
    if (!ref.current) return;
    const observer = new ResizeObserver((entries) => {
      setWidth(entries[0].contentRect.width);
    });
    observer.observe(ref.current);
    return () => observer.disconnect();
  }, [ref]);
  return width;
};

interface Props {
  data: UserData[];
}

export const UserList: FC<Props> = ({ data }) => {
  const [users, setUsers] = useState<UserData[]>(data);

  useEffect(() => {
    setUsers(data);
  }, [data]);

  useEffect(() => {
    const handleUpdate = (event: any) => {
      const { userName, isVisible } = event.detail;

      setUsers((currentUsers) =>
        currentUsers.map((u) => (u.userName === userName ? { ...u, visible: isVisible } : u)),
      );
    };

    window.addEventListener('visibility-updated', handleUpdate);

    return () => window.removeEventListener('visibility-updated', handleUpdate);
  }, []);

  const [searchQuery, setSearchQuery] = useState('');
  const navigate = useNavigate();
  const { user: loggedInUser } = useAuth();

  const [actionUser, setActionUser] = useState<string | null>(null);
  const [actionType, setActionType] = useState<'approve' | 'delete' | 'promote' | 'demote' | null>(
    null,
  );
  const [isActionLoading, setIsActionLoading] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const filteredData = useMemo(
    () => users.filter((user) => user.userName.toLowerCase().includes(searchQuery.toLowerCase())),
    [users, searchQuery],
  );

  const parentRef = useRef<HTMLDivElement>(null);
  const containerWidth = useContainerWidth(parentRef);

  const [listOffset, setListOffset] = useState(0);

  useLayoutEffect(() => {
    if (parentRef.current) {
      setListOffset(parentRef.current.offsetTop);
    }
  }, []);

  const CARD_WIDTH = 270;
  const GAP = 16;
  const columns = Math.max(1, Math.floor((containerWidth + GAP) / (CARD_WIDTH + GAP)));
  const rowCount = Math.ceil(filteredData.length / columns);

  const rowVirtualizer = useWindowVirtualizer({
    count: rowCount,
    estimateSize: () => 100,
    overscan: 8,

    scrollMargin: listOffset,
  });

  const handleAction = useCallback(
    (userName: string, type: 'approve' | 'delete' | 'promote' | 'demote') => {
      setActionError(null);
      setActionUser(userName);
      setActionType(type);
    },
    [],
  );
  const handleCardClick = useCallback(
    (userName: string) => navigate(`/games/${userName}`),
    [navigate],
  );
  const executeAction = async () => {
    if (!actionUser || !actionType) return;

    setIsActionLoading(true);
    setActionError(null);

    try {
      if (actionType === 'approve') {
        await approve(actionUser);

        setUsers((current) =>
          current.map((u) =>
            u.userName === actionUser ? { ...u, approved: true, canApprove: false } : u,
          ),
        );
      } else if (actionType === 'delete') {
        await deleteUser(actionUser);

        setUsers((current) => current.filter((u) => u.userName !== actionUser));
      } else if (actionType === 'promote') {
        await promoteToAdmin(actionUser);

        setUsers((current) =>
          current.map((u) => (u.userName === actionUser ? { ...u, admin: true } : u)),
        );
      } else if (actionType === 'demote') {
        await demoteAdmin(actionUser);
        setUsers((current) =>
          current.map((u) => (u.userName === actionUser ? { ...u, admin: false } : u)),
        );
      }

      setIsActionLoading(false);
      setActionUser(null);
      setActionType(null);
    } catch (error: any) {
      setIsActionLoading(false);
      let msg = 'An unexpected error occurred.';

      if (error.parsedBody) {
        msg =
          typeof error.parsedBody === 'string'
            ? error.parsedBody
            : error.parsedBody.message || JSON.stringify(error.parsedBody);
      } else if (error.statusText) {
        msg = error.statusText;
      } else if (error.message) {
        msg = error.message;
      }

      setActionError(msg);
    }
  };

  return (
    <Flex direction="column" align="center" width="100%" height="100%" p="2">
      {/* GAP FIX: Reduced mb from "5" to "3" to bring the list closer */}
      <Box width="100%" maxWidth="1100px" mb="3">
        <TextField.Root
          size="3"
          placeholder="Filter users..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          style={{ maxWidth: '400px', margin: '0 auto' }}
        >
          <TextField.Slot>
            <Search height="16" width="16" />
          </TextField.Slot>
        </TextField.Root>
      </Box>

      {/* VIRTUALIZED CONTAINER */}
      <Box
        ref={parentRef}
        style={{
          width: '100%',
          maxWidth: '1100px',
          position: 'relative',
          height: `${rowVirtualizer.getTotalSize()}px`,
        }}
      >
        {rowVirtualizer.getVirtualItems().map((virtualRow) => {
          const startIndex = virtualRow.index * columns;
          const rowUsers = filteredData.slice(startIndex, startIndex + columns);

          return (
            <Box
              key={virtualRow.key}
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: `${virtualRow.size}px`,
                transform: `translateY(${virtualRow.start - rowVirtualizer.options.scrollMargin}px)`,
              }}
            >
              <Flex gap={`${GAP}px`}>
                {rowUsers.map((user) => (
                  <Box key={user.userName} style={{ flex: 1, minWidth: 0 }}>
                    <UserCard
                      user={user}
                      loggedInUser={loggedInUser}
                      onNavigate={handleCardClick}
                      onAction={handleAction}
                    />
                  </Box>
                ))}
                {Array.from({ length: columns - rowUsers.length }).map((_, i) => (
                  <Box key={`filler-${i}`} style={{ flex: 1 }} />
                ))}
              </Flex>
            </Box>
          );
        })}
      </Box>

      {/* DIALOG LOGIC */}
      {actionUser && actionType && (
        <ConfirmDialog
          open={true}
          loading={isActionLoading}
          error={actionError}
          title={
            actionType === 'approve'
              ? 'Approve User'
              : actionType === 'promote'
                ? 'Promote to Admin'
                : actionType === 'demote'
                  ? 'Revoke Admin Privileges'
                  : 'Delete User'
          }
          description={
            actionType === 'approve' ? (
              <>
                Are you sure you want to approve <Strong>{actionUser}</Strong>?
              </>
            ) : actionType === 'promote' ? (
              <>
                Are you sure you want to grant <Strong>Admin</Strong> privileges to{' '}
                <Strong>{actionUser}</Strong>?
                <br />
                <br />
                They will have full access to manage users and settings.
              </>
            ) : actionType === 'demote' ? (
              <>
                Are you sure you want to revoke <Strong>Admin</Strong> privileges from{' '}
                <Strong>{actionUser}</Strong>?
                <br />
                <br />
                They will no longer be able to manage other users or access administrative settings.
              </>
            ) : (
              <>
                Are you sure you want to delete <Strong>{actionUser}</Strong>?<br />
                <br />
                This cannot be undone.
              </>
            )
          }
          confirmLabel={
            actionType === 'approve'
              ? 'Approve'
              : actionType === 'promote'
                ? 'Make Admin'
                : actionType === 'demote'
                  ? 'Revoke Admin'
                  : 'Delete'
          }
          confirmColor={
            actionType === 'approve' ? 'green' : actionType === 'promote' ? 'blue' : 'red'
          }
          onConfirm={executeAction}
          onCancel={() => {
            setActionUser(null);
            setActionType(null);
          }}
        />
      )}

      {filteredData.length === 0 && (
        <Text size="2" color="gray" mt="4">
          No users found.
        </Text>
      )}
    </Flex>
  );
};
