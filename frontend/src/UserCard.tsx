import { memo } from 'react';
import Tilt from 'react-parallax-tilt';
import { Card, Flex, Avatar, Text, Box, Tooltip, IconButton, DropdownMenu } from '@radix-ui/themes';
import { Shield, EyeOff, ShieldAlert, MoreVertical, Trash2, UserCheck, User, ShieldPlus, ShieldOff } from 'lucide-react';
import { UserData } from './User';

interface UserCardProps {
  user: UserData;
  loggedInUser: UserData | null;
  onNavigate: (userName: string) => void;
  onAction: (userName: string, type: 'approve') => void;
}


export const UserCard = memo(({ user, loggedInUser, onNavigate, onAction }: UserCardProps) => {
  return (
    <Tilt
      tiltMaxAngleX={5}
      tiltMaxAngleY={5}
      scale={1.02}
      transitionSpeed={400}
      style={{ transformStyle: 'preserve-3d' }}
    >
      <Card
        variant="surface"
        onClick={() => onNavigate(user.userName)}
        style={{
          cursor: 'pointer',
          height: '100%',
          position: 'relative',
          boxShadow: 'var(--shadow-6)',
        }}
      >
        {/* MAIN CONTAINER: Split into Left (Info) and Right (Icons) */}
        <Flex gap="3" align="center" justify="between">
          {/* LEFT: Avatar & Username */}
          <Flex gap="3" align="center" style={{ overflow: 'hidden' }}>
            <Avatar
              size="3"
              radius="full"
              fallback={user.userName.charAt(0).toUpperCase()}
              color="green"
            />
            <Text as="div" size="2" weight="bold" highContrast truncate>
              {user.userName}
            </Text>
          </Flex>

          {/* RIGHT: Status Icons & Menu Button */}
          <Flex align="center" gap="3" style={{ flexShrink: 0 }}>
            {/* Status Icons Group */}
            <Flex gap="2" align="center">
              {user.admin && (
                <Tooltip content="Admin">
                  <Shield size={16} color="var(--ruby-9)" fill="var(--ruby-4)" />
                </Tooltip>
              )}
              {!user.visible && (
                <Tooltip content="Hidden">
                  <EyeOff size={16} color="var(--gray-9)" />
                </Tooltip>
              )}
            </Flex>

            {/* Action Menu */}
            <Box onClick={(e) => e.stopPropagation()} style={{ display: 'flex' }}>
              <DropdownMenu.Root>
                <DropdownMenu.Trigger>
                  {/* CHANGED: Back to IconButton for proper hover/active states.
                      - variant="ghost": No background until hover.
                      - radius="full": Circular hover effect.
                      - highContrast: Ensures the icon color is strong.
                  */}
                  <IconButton
                    variant="ghost"
                    color="gray"
                    radius="full"
                    highContrast
                    style={{ cursor: 'pointer' }}
                  >
                    {/* REQUESTED: 25px Icon Size */}
                    <MoreVertical size={25} />
                  </IconButton>
                </DropdownMenu.Trigger>

                <DropdownMenu.Content align="end">
                  <DropdownMenu.Item onSelect={() => onNavigate(user.userName)}>
                    <User size={16} style={{ marginRight: 8 }} />
                    View Profile
                  </DropdownMenu.Item>

                  {user.canApprove && (
                    <DropdownMenu.Item
                      color="green"
                      onSelect={() => onAction(user.userName, 'approve')}
                    >
                      <UserCheck size={16} style={{ marginRight: 8 }} />
                      Approve User
                    </DropdownMenu.Item>
                  )}

                </DropdownMenu.Content>
              </DropdownMenu.Root>
            </Box>
          </Flex>
        </Flex>
      </Card>
    </Tilt>
  );
});
