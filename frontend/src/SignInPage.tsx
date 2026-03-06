import { FC, useState, ChangeEvent, FormEvent, useEffect } from 'react';
import {
  Card,
  Flex,
  Button,
  TextField,
  Text,
  Heading,
  Box,
  Link as RadixLink,
  Callout,
  Spinner,
} from '@radix-ui/themes';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { AlertTriangle, MailCheck } from 'lucide-react';
import { userLogin, canRegister } from './User';
import { useAuth } from './AuthContext';

export const SignInPage: FC = () => {
  const [canregister, setCanregister]
    = useState<boolean | null>(null);
  const [canregisterLoading, setCanregisterLoading] = useState(true);
  const { refreshUser } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const [error, setError] = useState<string | null>(null);
  const [resendStatus, setResendStatus] = useState<{ msg: string; color: 'green' | 'red' } | null>(
    null,
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isResending, setIsResending] = useState(false);

  const isFormInvalid = !username || !password;

  useEffect(() => {
    const doGetCanregister = async () => {
      if (canregisterLoading)
      {
        const b = await canRegister();
        setCanregister(b);
        setCanregisterLoading(false);
      }
    };
    doGetCanregister();
  });

  const handleUsernameInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setUsername(e.currentTarget.value);
    setError(null);
  };

  const handlePasswordInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPassword(e.currentTarget.value);
    setError(null);
  };

  const handleLoginSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (isFormInvalid) return;

    setIsSubmitting(true);
    setError(null);
    setResendStatus(null);

    try {
      const user = await userLogin(username, password);

      if (user && user.userName) {
        await refreshUser();
        navigate('/');
      } else {
        setError('Invalid username or password.');
      }
    } catch (err: any) {
      setError('An unexpected error occurred.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Box maxWidth="400px" mx="auto" pt="8">
      <Card
        size="4"
        variant="surface"
        style={{
          borderRadius: '16px',
          boxShadow: 'var(--shadow-6)',
          border: '1px solid rgba(255,255,255,0.1)',
        }}
      >
        <form onSubmit={handleLoginSubmit}>
          <Flex direction="column" gap="5">
            <Box mb="2">
              <Heading as="h2" size="6" trim="start" mb="1" align="center">
                Sign In
              </Heading>
              <Text as="p" size="2" color="gray" align="center">
                Enter your details to access your profile.
              </Text>
            </Box>

            {/* STANDARD ERROR DISPLAY */}
            {error && (
              <Callout.Root color="red" size="1">
                <Callout.Icon>
                  <AlertTriangle size={16} />
                </Callout.Icon>
                <Flex direction="column" gap="2">
                  <Callout.Text>{error}</Callout.Text>
                  <RadixLink asChild size="1" style={{ cursor: 'pointer' }}>
                    <RouterLink to="/forgot-password">Forgot password?</RouterLink>
                  </RadixLink>
                </Flex>
              </Callout.Root>
            )}

            <Flex direction="column" gap="4">
              <label>
                <Text as="div" size="2" mb="1" weight="bold">
                  Username
                </Text>
                <TextField.Root
                  size="3"
                  placeholder="Enter your username"
                  name="username"
                  value={username}
                  onChange={handleUsernameInputChange}
                  color={error ? 'red' : undefined}
                />
              </label>

              <label>
                <Text as="div" size="2" mb="1" weight="bold">
                  Password
                </Text>
                <TextField.Root
                  size="3"
                  type="password"
                  placeholder="••••••••"
                  name="password"
                  value={password}
                  onChange={handlePasswordInputChange}
                  color={error ? 'red' : undefined}
                />
              </label>
            </Flex>

            <Flex direction="column" gap="3" mt="2">
              <Button
                size="3"
                variant="surface"
                type="submit"
                disabled={isFormInvalid || isSubmitting}
                style={{ cursor: isFormInvalid || isSubmitting ? 'not-allowed' : 'pointer' }}
              >
                {isSubmitting ? (
                  <>
                    <Spinner />
                    Signing in…
                  </>
                ) : (
                  'Login'
                )}
              </Button>

              {canregister && (
                <Text size="2" align="center" color="gray">
                  New here?{' '}
                  <RadixLink asChild highContrast>
                    <RouterLink to="/register" style={{ cursor: 'pointer' }}>
                      Create an account
                    </RouterLink>
                  </RadixLink>
                </Text>
              )}
            </Flex>
          </Flex>
        </form>
      </Card>
    </Box>
  );
};
