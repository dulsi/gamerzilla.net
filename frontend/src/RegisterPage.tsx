import { useEffect, useState, useMemo, FC, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { userRegister, canRegister } from './User';
import { useAuth } from './AuthContext';
import {
  Box,
  Card,
  Flex,
  Heading,
  TextField,
  Button,
  Link as RadixLink,
  Text,
  Callout,
  Spinner,
  Progress,
} from '@radix-ui/themes';
import { Info } from 'lucide-react';
import { getPasswordStrength } from './PasswordStrength';
import { Link as RouterLink } from 'react-router-dom';

export const RegisterPage: FC = () => {
  const navigate = useNavigate();
  const { refreshUser } = useAuth();

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [passwordVerify, setPasswordVerify] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isPending, setIsPending] = useState(false);

  
  const strength = useMemo(() => getPasswordStrength(password), [password]);
  const doPasswordsMatch = !passwordVerify || password === passwordVerify;

  
  
  const isFormValid =
    username.trim() !== '' &&
    password !== '' &&
    doPasswordsMatch &&
    strength.score >= 2;

const handleRegisterSubmit = async (e: FormEvent<HTMLFormElement>) => {
  e.preventDefault();
  if (!isFormValid) return;

  setError(null);
  setIsLoading(true);

  try {
    
    const result = await userRegister(username, password);

    await refreshUser();
    navigate('/');
  } catch (err: any) {
    console.error('Registration Error:', err);
    let errorMessage = 'An unexpected error occurred.';

    
    if (err.parsedBody) {
      errorMessage =
        typeof err.parsedBody === 'string'
          ? err.parsedBody
          : err.parsedBody.message || JSON.stringify(err.parsedBody);
    }
    
    else if (err instanceof Error) {
      errorMessage = err.message;
    }
    
    else if (typeof err === 'string') {
      errorMessage = err;
    }

    
    const lowerMsg = errorMessage.toLowerCase();
    if (lowerMsg.includes('user exists')) {
      setError('User already exists. Please choose a different username.');
    } else {
      setError(errorMessage);
    }
  } finally {
    setIsLoading(false);
  }
};

  
  const RequiredMark = () => (
    <Text color="red" style={{ marginLeft: '2px' }}>
      *
    </Text>
  );


  if (isPending) {
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
          <Flex direction="column" gap="4" align="center" py="4">
            <Heading size="6">Wait for account to be approved</Heading>
            <Callout.Root color="blue" role="alert">
              <Callout.Icon>
                <Info size={16} />
              </Callout.Icon>
              <Callout.Text>
                Once admin manually approves you will be able to upload your achievements.
              </Callout.Text>
            </Callout.Root>
            <Button variant="surface" asChild mt="2">
              <RouterLink to="/signin" style={{ cursor: 'pointer' }}>
                Back to Sign In
              </RouterLink>
            </Button>
          </Flex>
        </Card>
      </Box>
    );
  }

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
        <form onSubmit={handleRegisterSubmit}>
          <Flex direction="column" gap="5">
            <Box mb="2">
              <Heading as="h2" size="6" trim="start" mb="1" align="center">
                Create Account
              </Heading>
              <Text as="p" size="2" color="gray" align="center">
                Sign up to start tracking your achievements.
              </Text>
            </Box>

            {error && (
              <Callout.Root color="red" role="alert">
                <Callout.Icon>
                  <Info size={16} />
                </Callout.Icon>
                <Callout.Text>{error}</Callout.Text>
              </Callout.Root>
            )}

            <Flex direction="column" gap="4">
              <label>
                <Text as="div" size="2" mb="1" weight="bold">
                  Username <RequiredMark />
                </Text>
                <TextField.Root
                  size="3"
                  placeholder="Choose a username"
                  value={username}
                  onChange={(e) => setUsername(e.currentTarget.value)}
                  disabled={isLoading}
                />
              </label>

              <Box>
                <label>
                  <Text as="div" size="2" mb="1" weight="bold">
                    Password <RequiredMark />
                  </Text>
                  <TextField.Root
                    size="3"
                    type="password"
                    placeholder="••••••••"
                    value={password}
                    onChange={(e) => setPassword(e.currentTarget.value)}
                    disabled={isLoading}
                  />
                </label>
                {password.length > 0 && (
                  <Box mt="2">
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
              </Box>

              <Box>
                <label>
                  <Text as="div" size="2" mb="1" weight="bold">
                    Confirm Password <RequiredMark />
                  </Text>
                  <TextField.Root
                    size="3"
                    type="password"
                    placeholder="••••••••"
                    value={passwordVerify}
                    onChange={(e) => setPasswordVerify(e.currentTarget.value)}
                    disabled={isLoading}
                    color={!doPasswordsMatch ? 'red' : undefined}
                  />
                </label>
                {!doPasswordsMatch && (
                  <Text color="red" size="1" mt="1" as="p">
                    Passwords do not match.
                  </Text>
                )}
              </Box>
            </Flex>

            <Flex direction="column" gap="3" mt="2">
              <Button
                size="3"
                variant="surface"
                type="submit"
                disabled={isLoading || !isFormValid}
                style={{ cursor: isLoading || !isFormValid ? 'not-allowed' : 'pointer' }}
              >
                <Spinner loading={isLoading}>{isLoading ? 'Registering...' : 'Register'}</Spinner>
              </Button>

              <Text size="2" align="center" color="gray">
                Already have an account?{' '}
                <RadixLink asChild highContrast>
                  <RouterLink
                    to="/signin"
                    style={{ cursor: 'pointer', pointerEvents: isLoading ? 'none' : 'auto' }}
                  >
                    Sign In
                  </RouterLink>
                </RadixLink>
              </Text>
            </Flex>
          </Flex>
        </form>
      </Card>
    </Box>
  );
};
