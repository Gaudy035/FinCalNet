interface NavbarLinkProps {
  linkClick: () => void;
  linkText: string;
}

export default function NavbarLink({ linkClick, linkText }: NavbarLinkProps) {
  return (
    <button
      type='button'
      onClick={linkClick}
      className='cursor-pointer underline-offset-3 hover:underline'
    >
      {linkText}
    </button>
  );
}
