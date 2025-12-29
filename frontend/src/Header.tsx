import { Link, useNavigate } from 'react-router-dom';
import { Flex, Text, Button, DropdownMenu, Avatar, Box, Heading, Card } from '@radix-ui/themes';
import { Eye, EyeOff, Gamepad2, LogOut, Settings, Users } from 'lucide-react';
import { useAuth } from './AuthContext'; 
import { visible } from './User';
import { useState } from 'react';
import { AccountSettingsDialog } from './AccountSettingsDialog';


export const Header = () => {
  const navigate = useNavigate();
  
  const { user, logout, refreshUser } = useAuth();
  const [settingsOpen, setSettingsOpen] = useState(false);

  const handleLogout = async () => {
    await logout();
    navigate('/signin');
  };

    const handleMyGames = async () => {
      navigate(`/games/${user?.userName}`);
    };

        const handleUsers = async () => {
          navigate('/');
        };

const handleToggleVisible = async () => {
  if (user) {
    const newStatus = user.visible ? 0 : 1;
    const success = await visible(user.userName, newStatus);

    if (success) {
      await refreshUser(); 

      
      window.dispatchEvent(
        new CustomEvent('visibility-updated', {
          detail: { userName: user.userName, isVisible: newStatus === 1 },
        }),
      );
    }
  }
};

  return (
    <Card
      style={{
        boxShadow: 'var(--shadow-6)',
      }}
    >
      {/* ... Logo Section (Same as before) ... */}
      <Flex align="center" justify="between" height="100%">
        <Box>
          <Link
            to="/"
            style={{ textDecoration: 'none', display: 'flex', alignItems: 'center', gap: '8px' }}
          >
            <Gamepad2 size={28} color="var(--accent-9)" />
            <Heading size="5" weight="bold" style={{ color: 'var(--gray-12)' }}>
              Gamerzilla
            </Heading>
          </Link>
        </Box>

        <Box>
          {/* Check Context User instead of local state */}
          {!user || user.userName === '' ? (
            <Link to="/signin" style={{ textDecoration: 'none' }}>
              <Button variant="ghost" size="3" style={{ cursor: 'pointer' }}>
                <Avatar
                  size="1"
                  radius="full"
                  fallback="?"
                  color="gray"
                  style={{ marginRight: 8 }}
                />
                Sign In
              </Button>
            </Link>
          ) : (
            <DropdownMenu.Root>
              <DropdownMenu.Trigger>
                <Button variant="ghost" size="3" color="gray" style={{ cursor: 'pointer' }}>
                  <Avatar
                    size="2"
                    radius="full"
                    fallback={user.userName[0].toUpperCase()}
                    color="green"
                  />
                  <Text weight="medium">{user.userName}</Text>
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Content size="2" align="end">
                {/* 1. Account Settings */}

                <DropdownMenu.Item onSelect={handleUsers}>
                  <Users size={16} />
                  Users
                </DropdownMenu.Item>
                <DropdownMenu.Item onSelect={handleMyGames}>
                  <Gamepad2 size={16} />
                  Games
                </DropdownMenu.Item>
                <DropdownMenu.Item onSelect={() => setSettingsOpen(true)}>
                  <Settings size={16} />
                  Settings
                </DropdownMenu.Item>
                <DropdownMenu.Item onSelect={handleToggleVisible}>
                  <Flex align="center" gap="2">
                    {/* Logic: If currently visible, show 'EyeOff' to indicate hiding action */}
                    {user.visible ? <EyeOff size={16} /> : <Eye size={16} />}
                    {user.visible ? 'Turn Invisible' : 'Turn Visible'}
                  </Flex>
                </DropdownMenu.Item>
                <DropdownMenu.Separator />
                <DropdownMenu.Item color="red" onSelect={handleLogout}>
                  <LogOut size={16} />
                  Logout
                </DropdownMenu.Item>
              </DropdownMenu.Content>
            </DropdownMenu.Root>
          )}
          <AccountSettingsDialog open={settingsOpen} onOpenChange={setSettingsOpen} />
        </Box>
      </Flex>
    </Card>
  );
};
