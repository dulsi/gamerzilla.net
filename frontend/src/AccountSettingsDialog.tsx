import { useState, useMemo, useEffect } from 'react';
import { Dialog, Button, Flex, Text, TextField, Callout, Progress, Box, Spinner } from '@radix-ui/themes';
import { useAuth } from './AuthContext';
import { changePassword, updateEmail } from './AccountApi';
import { getPasswordStrength } from './PasswordStrength';
import { defaultServerOptions, getServerOptions } from './User';




export const AccountSettingsDialog = ({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (o: boolean) => void;
}) => {
  const { user } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
    const [emailUpdating, setEmailUpdating] = useState(false);
  const [serverOpts, setServerOpts] = useState(defaultServerOptions);

  useEffect(() => {
    getServerOptions().then(setServerOpts);
  }, []);

  
  const [oldPass, setOldPass] = useState('');
  const [newPass, setNewPass] = useState('');
  const [confirmPass, setConfirmPass] = useState('');
  const [passMsg, setPassMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  
  const [newEmailAddress, setNewEmailAddress] = useState('');
  const [emailMsg, setEmailMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(
    null,
  );

  
  const strength = useMemo(() => getPasswordStrength(newPass), [newPass]);
  const doPasswordsMatch = !confirmPass || newPass === confirmPass;
    const isPasswordSame = (oldPass === newPass) && oldPass.length > 0;

  
  const canSubmitPassword =
    oldPass.length > 0 && newPass.length > 0 && doPasswordsMatch && strength.score >= 2;

  const handlePasswordUpdate = async () => {
    setPassMsg(null);
    setIsLoading(true);
    try {
      const msg = await changePassword(oldPass, newPass);
      setPassMsg({ type: 'success', text: msg });
      setOldPass('');
      setNewPass('');
      setConfirmPass('');
    } catch (error: any) {
      setPassMsg({ type: 'error', text: error.message || 'An error occurred.' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleEmailUpdate = async () => {
    if (!newEmailAddress || !newEmailAddress.includes('@')) {
      setEmailMsg({ type: 'error', text: 'Please enter a valid email address.' });
      return;
    }
    setEmailMsg(null);
    setEmailUpdating(true);
    try {
      const msg = await updateEmail(newEmailAddress);
      setEmailMsg({ type: 'success', text: msg });
      setNewEmailAddress('');
    } catch (error: any) {
      setEmailMsg({ type: 'error', text: error.message || 'An error occurred.' });
    } finally {
      setEmailUpdating(false);
    }
  };

    const isEmailValid = useMemo(() => {
      const emailRegex = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i;

      if (!serverOpts.emailEnabled) return true; 
      return newEmailAddress.trim() !== '' && emailRegex.test(newEmailAddress) && user?.email !== newEmailAddress;
    }, [newEmailAddress, serverOpts.emailEnabled]);

  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Content style={{ maxWidth: 450 }} onInteractOutside={(e) => e.preventDefault()}>
        <Dialog.Title>Account Settings</Dialog.Title>

        <Flex direction="column" gap="4">
          {serverOpts.allowPasswordChange && (
            <Flex direction="column" gap="2">
              <Text size="3" weight="bold">
                Change Password
              </Text>

              {/* 1. Current Password */}
              <label>
                <Text as="div" size="1" mb="1" weight="bold" color="gray">
                  Current Password
                </Text>
                <TextField.Root
                  placeholder="Enter current password"
                  type="password"
                  value={oldPass}
                  onChange={(e) => setOldPass(e.target.value)}
                  
                  color={!oldPass && newPass.length > 0 ? 'red' : undefined}
                />
              </label>

              {/* NEW: Explicit Error Text */}
              {!oldPass && newPass.length > 0 && (
                <Text color="red" size="1">
                  Current password is required to make changes.
                </Text>
              )}

              {/* 2. New Password */}
              <label>
                <Text as="div" size="1" mb="1" weight="bold" color="gray">
                  New Password
                </Text>
                <TextField.Root
                  placeholder="Enter new password"
                  type="password"
                  value={newPass}
                  onChange={(e) => {
                    setNewPass(e.target.value);
                    if (passMsg?.type === 'error') setPassMsg(null);
                  }}
                />
              </label>

              {/* STRENGTH METER */}
              {newPass.length > 0 && (
                <Box mb="2">
                  <Flex justify="between" mb="1">
                    <Text size="1" color={strength.color}>
                      {strength.label}
                    </Text>
                    <Text size="1" color="gray">
                      Must be at least 'Fair'
                    </Text>
                  </Flex>
                  <Progress value={strength.score * 33.3} color={strength.color} size="1" />
                </Box>
              )}

              {/* 3. Confirm Password */}
              <label>
                <Text as="div" size="1" mb="1" weight="bold" color="gray">
                  Confirm Password
                </Text>
                <TextField.Root
                  placeholder="Retype new password"
                  type="password"
                  value={confirmPass}
                  onChange={(e) => setConfirmPass(e.target.value)}
                  color={!doPasswordsMatch ? 'red' : undefined}
                />
              </label>

              {!doPasswordsMatch && (
                <Text color="red" size="1">
                  Passwords do not match.
                </Text>
              )}

              {isPasswordSame && (
                <Text color="red" size="1">
                  Cannot set new password to the same value as old one.
                </Text>
              )}

              <Button
                onClick={handlePasswordUpdate}
                disabled={!canSubmitPassword || isLoading || emailUpdating || isPasswordSame}
              >
                {isLoading ? 'Updating...' : 'Update Password'}
              </Button>

              {passMsg && (
                <Callout.Root color={passMsg.type === 'error' ? 'red' : 'green'}>
                  <Callout.Text>{passMsg.text}</Callout.Text>
                </Callout.Root>
              )}
            </Flex>
          )}

          <div style={{ borderTop: '1px solid var(--gray-6)' }} />

          {/* --- CHANGE EMAIL SECTION --- */}
          <Flex direction="column" gap="2">
            <Text size="3" weight="bold">
              Update Email
            </Text>
            <Text size="1" color="gray">
              Current: {user?.email || 'None'}
            </Text>
            <TextField.Root
              placeholder="New Email Address"
              value={newEmailAddress}
              onChange={(e) => setNewEmailAddress(e.target.value)}
            />
            <Button
              variant="soft"
              onClick={handleEmailUpdate}
              disabled={!isEmailValid || emailUpdating || isLoading}
            >
              <Spinner loading={emailUpdating}>
                {isLoading
                  ? serverOpts.emailEnabled
                    ? 'Sending...'
                    : 'Updating...'
                  : serverOpts.emailEnabled
                    ? 'Send Verification Link'
                    : 'Update Immediately'}
              </Spinner>
            </Button>

            {emailMsg && (
              <Callout.Root color={emailMsg.type === 'error' ? 'red' : 'green'}>
                <Callout.Text>{emailMsg.text}</Callout.Text>
              </Callout.Root>
            )}
          </Flex>
        </Flex>

        <Flex gap="3" mt="4" justify="end">
          <Dialog.Close>
            <Button variant="soft" color="gray">
              Close
            </Button>
          </Dialog.Close>
        </Flex>
      </Dialog.Content>
    </Dialog.Root>
  );
};
