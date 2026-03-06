export const getPasswordStrength = (pass: string) => {
  let score = 0;
  if (!pass) return { score: 0, label: '', color: 'gray' as const };

  if (pass.length > 7) score += 1;
  if (pass.length > 10) score += 1;
  if (/[A-Z]/.test(pass)) score += 1;
  if (/[0-9]/.test(pass)) score += 1;
  if (/[^A-Za-z0-9]/.test(pass)) score += 1;

  switch (score) {
    case 0:
    case 1:
    case 2:
      return { score: 1, label: 'Weak', color: 'red' as const };
    case 3:
    case 4:
      return { score: 2, label: 'Fair', color: 'amber' as const };
    case 5:
      return { score: 3, label: 'Strong', color: 'green' as const };
    default:
      return { score: 0, label: '', color: 'gray' as const };
  }
};