<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.Cryptography</sub>

# CodeBrix.Cryptography

**CodeBrix.Cryptography is a fully managed, cross-platform, general-purpose cryptography library.** It
covers ASN.1 encoding, a very broad set of symmetric and asymmetric ciphers, digests, MACs, signatures
and key agreement, key derivation and password hashing, post-quantum algorithms, TLS and DTLS, OpenPGP,
CMS and S/MIME structures, PKCS, CMP, CRMF, OCSP, TSP, X.509 certificate and CRL handling with path
validation, and PEM interoperability. Use it from any .NET 10 application, or from a CodeBrix.Platform
application - it has no dependencies of its own and needs nothing installed on the machine.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.Cryptography](https://github.com/ellisnet/CodeBrix.Cryptography) |
| **Packages** | [`CodeBrix.Cryptography.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Cryptography.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later, and nothing else. No NuGet dependencies, no native libraries, no P/Invoke and no platform-specific assets |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Anywhere .NET 10 runs. AES-NI, carry-less multiply and similar x86 intrinsics are used opportunistically when the running CPU has them, and fall back to portable managed code otherwise |

## What it does

- **Block and stream ciphers** - AES with x86 acceleration, ARIA, Blowfish, Camellia, CAST5 and CAST6,
  ChaCha20, DES and Triple-DES, GOST 28147 and Kuznyechik, IDEA, Noekeon, RC2, RC4, RC5, RC6,
  Rijndael, SEED, Serpent, Salsa20, SM4, Threefish, Twofish, XSalsa20 and more.
- **Modes and padding** - CBC, CCM, CFB, CTS, EAX, ECB, GCM with carry-less-multiply acceleration,
  GOFB, KCCM, KGCM, OCB, OFB, OpenPGP CFB and SIC/CTR, plus PKCS#7, ISO 7816-4, ISO 10126-2, TBC,
  X9.23 and zero-byte padding.
- **Digests and extendable-output functions** - SHA-1, the SHA-2 family, SHA-3, SHAKE, cSHAKE, KMAC,
  TupleHash, ParallelHash, BLAKE2b and BLAKE2s with x86 paths, BLAKE3, Haraka, Keccak, MD2, MD4, MD5,
  RIPEMD, SM3, Skein, Whirlpool, GOST3411 and the DSTU digests.
- **MACs** - CBC-MAC, CMAC, GMAC, HMAC, KGMAC, Poly1305, SipHash, SkeinMac, DSTU7564Mac and more.
- **Public-key cryptography** - RSA, DSA, Diffie-Hellman, ElGamal, ECDSA, ECDH and ECMQV, ECGOST,
  EdDSA over Ed25519 and Ed448, X25519 and X448, SM2, with the associated key generation, key
  agreement and signature schemes.
- **Post-quantum cryptography** - ML-KEM, ML-DSA, SLH-DSA, LMS/HSS, XMSS, Falcon, BIKE, HQC, Classic
  McEliece, Frodo, NTRU and NTRU Prime, Picnic, SABER and SNOVA.
- **Key derivation and password hashing** - Argon2, bcrypt, scrypt, HKDF, PBKDF1 and PBKDF2, the
  PKCS#12 KDF, the concatenation and X9.63 KDFs, and the TLS and SSL pseudo-random functions.
- **TLS and DTLS** - a full TLS 1.0 to 1.3 and DTLS 1.0 to 1.2 client and server implementation, with
  pluggable crypto, pre-shared keys, SRP, raw public keys and certificate-type negotiation.
- **OpenPGP** - key ring generation and management, encryption, signing, compression, ASCII armor and
  cleartext-signed messages.
- **CMS and S/MIME, PKCS and certificates** - signed, enveloped, digested, encrypted and compressed
  CMS data, PKCS#1, #5, #7, #8, #10 and #12, X.509 certificate, attribute-certificate and CRL
  generation, parsing and PKIX path validation, OCSP, timestamping, CMP and CRMF.
- **ASN.1** - DER, BER and DL encoding and parsing, and the object-identifier and structure
  definitions for X.509, PKCS, CMS, CMP, CRMF, OCSP, TSP, X9.62, NIST, SEC and many other
  specifications.
- **Interoperability** - PEM reading and writing including encrypted private keys, OpenSSH public and
  private key blobs, Java keystore reading, format-preserving encryption, streaming cipher, digest,
  MAC and signer wrappers, and explicit conversion to and from `System.Security.Cryptography` types.

## When to use it

Reach for CodeBrix.Cryptography when the base class library stops short: a cipher, curve, digest or
signature scheme it does not have, a certificate or CRL you must *generate* rather than merely read, a
PKCS#12 store you must build, a PKIX path you must validate against your own trust anchors, OpenPGP or
CMS structures, PEM interoperability, a TLS or DTLS stack you drive yourself, or a post-quantum
algorithm. It is a toolbox, not a facade - it hands you primitives and expects you to compose them.

When the task is narrower, something else in the family is a better fit.
[CodeBrix.Sqlite](CodeBrix.Sqlite.md) already encrypts database columns with a ready-made AES-GCM
engine, and [CodeBrix.SSH](CodeBrix.SSH.md) already speaks SSH-2 and reads every private key format
that matters there - and it reaches this library for its key agreement, Ed25519, ML-KEM, ChaCha20 and
Poly1305, Argon2 and PKCS#8 decryption, so nothing is lost by starting there.

What it does not do:

- It does not integrate with `System.Security.Cryptography` as a provider. There is no configuration
  registration and no way to make `RSA.Create()` or `Aes.Create()` return these implementations. The
  bridge is manual and explicit, through `Security.DotNetUtilities`, which converts keys and
  certificates between the two worlds.
- It does not use operating-system key stores, hardware tokens, TPMs, PKCS#11 devices or
  operating-system certificate stores. Keys live in managed memory, and zeroing and protecting them is
  your job. Trust anchors for path validation are supplied by you; the library has no notion of "the
  system root store".
- It does not fetch anything over the network. There is no OCSP client, no CRL downloader, no
  certificate-chain chasing and no key-server lookup. It builds and parses the messages; transport is
  yours. TLS and DTLS are the exception in that they drive an existing `Stream` or datagram transport
  you provide, and never open a socket themselves.
- It does not do S/MIME MIME handling. CMS objects are produced and consumed; the multipart packaging
  around them is not part of the package.
- It is not FIPS-validated.
- It has no ambient configuration file, no dependency-injection registration and no logging. Behavior
  is controlled by constructor arguments and the thread and environment properties described under
  [Runtime configuration](#runtime-configuration).

> [!WARNING]
> The post-quantum implementations should all be considered experimental and subject to change. That
> covers everything under `CodeBrix.Cryptography.Pqc.*`, plus the ML-KEM, ML-DSA and SLH-DSA support in
> `CodeBrix.Cryptography.Crypto`. Treat their APIs, parameter sets and encodings as unstable across
> releases: do not use them to protect data that has to remain readable after an upgrade, and do not
> assume a key or signature produced by one release will be parseable by the next. The classical
> algorithms carry no such caveat.

## Getting started

```bash
dotnet add package CodeBrix.Cryptography.MitLicenseForever
```

The package ID carries the license suffix; the namespaces do not. There is no single entry-point type
and no initialization call - no provider registration, no native asset copy step. Namespaces map
one-to-one onto feature areas, so you take the usings for the area you are working in:

```csharp
// Symmetric encryption
using CodeBrix.Cryptography.Crypto;              // AesUtilities, IDigest, ...
using CodeBrix.Cryptography.Crypto.Engines;
using CodeBrix.Cryptography.Crypto.Modes;
using CodeBrix.Cryptography.Crypto.Paddings;
using CodeBrix.Cryptography.Crypto.Parameters;
using CodeBrix.Cryptography.Security;            // SecureRandom

// Asymmetric keys, signatures and certificates
using CodeBrix.Cryptography.Asn1.X509;           // X509Name, KeyUsage, ...
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Crypto.Operators;    // Asn1SignatureFactory
using CodeBrix.Cryptography.Crypto.Parameters;
using CodeBrix.Cryptography.Math;                // BigInteger
using CodeBrix.Cryptography.Security;
using CodeBrix.Cryptography.X509;

// PEM interop
using CodeBrix.Cryptography.OpenSsl;
using CodeBrix.Cryptography.Security;

// OpenPGP
using CodeBrix.Cryptography.Bcpg;
using CodeBrix.Cryptography.Bcpg.OpenPgp;

// TLS
using CodeBrix.Cryptography.Tls;
using CodeBrix.Cryptography.Tls.Crypto;
using CodeBrix.Cryptography.Tls.Crypto.Impl.BC;
```

Encrypting one message with an authenticated cipher is the shortest useful program the library
supports, and it exercises the three things every other recipe repeats: one `SecureRandom`, an engine
chosen at run time, and an output buffer sized by the cipher itself.

```csharp
using System;
using System.Text;
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Digests;
using CodeBrix.Cryptography.Crypto.Modes;
using CodeBrix.Cryptography.Crypto.Parameters;
using CodeBrix.Cryptography.Security;
using CodeBrix.Cryptography.Utilities.Encoders;

internal static class Program
{
    private static void Main()
    {
        var random = new SecureRandom();

        byte[] key = SecureRandom.GetNextBytes(random, 32);
        byte[] nonce = SecureRandom.GetNextBytes(random, 12);
        byte[] plaintext = Encoding.UTF8.GetBytes("hello, cryptography");

        var cipher = new GcmBlockCipher(AesUtilities.CreateEngine());
        cipher.Init(forEncryption: true,
            new AeadParameters(new KeyParameter(key), 128, nonce));

        byte[] ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
        int n = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
        n += cipher.DoFinal(ciphertext, n);

        Console.WriteLine("ciphertext+tag: " + Hex.ToHexString(ciphertext, 0, n));
        Console.WriteLine("sha-256:        " +
            Hex.ToHexString(DigestUtilities.CalculateDigest("SHA-256", plaintext)));
        Console.WriteLine("aes-ni:         " + AesUtilities.IsHardwareAccelerated);
    }
}
```

Notice `AesUtilities.CreateEngine()` rather than a constructor: it returns the hardware-accelerated
engine when the CPU supports it, and `IsHardwareAccelerated` tells you which one you got.

<details>
<summary>The minimum viable project</summary>

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CodeBrix.Cryptography.MitLicenseForever" />
  </ItemGroup>

</Project>
```

</details>

## Key concepts

### Randomness

`SecureRandom`, in `CodeBrix.Cryptography.Security`, is the source every generator in the library
expects. Prefer it over `System.Random`, and reuse one instance: constructing one seeds a deterministic
random bit generator, so constructing many in a loop is pure overhead. It exposes `GenerateSeed`,
`SetSeed`, `NextBytes`, `NextInt`, `NextLong`, the static `GetNextBytes` and `Fill` helpers, and a
`GetInstance(algorithm)` factory. `CryptoServicesRegistrar.GetSecureRandom()` returns a shared
instance, and its overload returns the argument when it is non-null and the shared instance otherwise -
useful when a `SecureRandom` parameter is optional. Deterministic and NIST-specified generators live
in `Crypto.Prng` and `Crypto.Prng.Drbg`.

### Authenticated encryption

Every AEAD cipher implements `IAeadCipher` in `Crypto.Modes` and shares one shape: `Init`,
`ProcessAadBytes`, `ProcessBytes`, `DoFinal`, `GetMac` and `Reset`, with `GetOutputSize` and
`GetUpdateOutputSize` for buffer sizing.

```csharp
var cipher = new GcmBlockCipher(AesUtilities.CreateEngine());
cipher.Init(forEncryption: true,
    new AeadParameters(new KeyParameter(key), 128, nonce, associatedData));
byte[] output = new byte[cipher.GetOutputSize(input.Length)];
int n = cipher.ProcessBytes(input, 0, input.Length, output, 0);
n += cipher.DoFinal(output, n);
```

The `macSize` argument is in **bits**. On decryption, `DoFinal` throws
`InvalidCipherTextException` when the tag does not verify - that exception *is* the authentication
failure, so never swallow it and never keep the partially written buffer. The family is
`GcmBlockCipher`, `GcmSivBlockCipher` (nonce-misuse resistant), `CcmBlockCipher`, `EaxBlockCipher`,
`OcbBlockCipher`, `KCcmBlockCipher`, `ChaCha20Poly1305`, `XChaCha20Poly1305` with its 24-byte nonce,
and `AsconAead128`.

### Classic modes and padding

Composition is explicit: an engine goes inside a mode, and the mode goes inside a buffered cipher that
applies padding.

```csharp
var cipher = new PaddedBufferedBlockCipher(
    new CbcBlockCipher(AesUtilities.CreateEngine()), new Pkcs7Padding());
cipher.Init(forEncryption: true,
    new ParametersWithIV(new KeyParameter(key), iv));
byte[] output = new byte[cipher.GetOutputSize(input.Length)];
int n = cipher.ProcessBytes(input, 0, input.Length, output, 0);
n += cipher.DoFinal(output, n);
```

Modes live in `Crypto.Modes` - `CbcBlockCipher`, `CfbBlockCipher`, `OfbBlockCipher`, `SicBlockCipher`
for CTR, `EcbBlockCipher`, `CtsBlockCipher` and the GOST and OpenPGP variants - and paddings in
`Crypto.Paddings`: `Pkcs7Padding`, `ISO7816d4Padding`, `ISO10126d2Padding`, `X923Padding`,
`TbcPadding` and `ZeroBytePadding`.

### Digests, XOFs and MACs

`IDigest` and `IMac` share a shape, and both are resolvable by name or OID through the facade in
`CodeBrix.Cryptography.Security`.

```csharp
IDigest digest = new Sha256Digest();               // Crypto.Digests
digest.BlockUpdate(data, 0, data.Length);
byte[] hash = new byte[digest.GetDigestSize()];
digest.DoFinal(hash, 0);

IMac mac = new HMac(new Sha256Digest());           // Crypto.Macs
mac.Init(new KeyParameter(key));
mac.BlockUpdate(data, 0, data.Length);
byte[] tag = new byte[mac.GetMacSize()];
mac.DoFinal(tag, 0);
```

`DigestUtilities.GetDigest("SHA-256")` and `MacUtilities.GetMac("HMACSHA256")` do the same by name,
with `CalculateDigest` and `CalculateMac` one-shot helpers alongside them.

### Key derivation and password hashing

Password KDFs are slow on purpose. Argon2 is built through a parameter builder and then a generator:

```csharp
var argon2Params = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
    .WithVersion(Argon2Parameters.Version13)
    .WithSalt(salt).WithIterations(3).WithParallelism(4)
    .WithMemoryAsKB(65536).Build();
var argon2 = new Argon2BytesGenerator();
argon2.Init(argon2Params);
byte[] derived = new byte[32];
argon2.GenerateBytes(passwordBytes, derived);
```

PBKDF2 is `Pkcs5S2ParametersGenerator`, and its key and IV sizes are in **bits**:

```csharp
var pbkdf2 = new Pkcs5S2ParametersGenerator(new Sha256Digest());
pbkdf2.Init(PbeParametersGenerator.Pkcs5PasswordToUtf8Bytes(password),
            salt, 600_000);
var keyParam = (KeyParameter)pbkdf2.GenerateDerivedParameters(256);
```

`SCrypt.Generate`, `BCrypt.Generate` and `OpenBsdBCrypt` cover the other password KDFs;
`HkdfBytesGenerator` covers HKDF, and `Kdf1BytesGenerator`, `Kdf2BytesGenerator`,
`Mgf1BytesGenerator` and the SP 800-108 counter, feedback and double-pipeline generators cover the
rest. Tune the work factor to your hardware budget and measure it; do not raise it blindly, and do not
lower it to "fix" a slow login.

### RSA

`RsaKeyPairGenerator` with `KeyGenerationParameters` produces an `AsymmetricCipherKeyPair`;
`RsaKeyGenerationParameters` is the explicit-control variant when you want to choose the public
exponent and certainty. Raw RSA is never used bare - wrap the engine in a padding scheme, and prefer
`RSABlindedEngine` for private-key operations, which is the side-channel-hardened variant.

```csharp
var rsa = new OaepEncoding(new RSABlindedEngine(), new Sha256Digest());
rsa.Init(forEncryption: true, pair.Public);
byte[] wrapped = rsa.ProcessBlock(sessionKey, 0, sessionKey.Length);
```

RSA can only encrypt up to `GetInputBlockSize()` bytes, so use it to wrap a symmetric key, not to
encrypt a message.

### Signatures

`ISigner` is the facade, and `SignerUtilities` resolves one by mechanism name or OID.

```csharp
ISigner signer = SignerUtilities.InitSigner("SHA256withRSA", forSigning: true,
                                            pair.Private, random);
signer.BlockUpdate(data, 0, data.Length);
byte[] signature = signer.GenerateSignature();

ISigner verifier = SignerUtilities.InitSigner("SHA256withRSA", forSigning: false,
                                              pair.Public, null);
verifier.BlockUpdate(data, 0, data.Length);
bool ok = verifier.VerifySignature(signature);
```

Mechanism names include `"SHA256withRSA"`, `"SHA384withRSA"`, `"SHA512withRSA"`,
`"SHA256withRSAandMGF1"` for PSS, `"SHA256withECDSA"`, `"SHA256withDSA"`, `"Ed25519"`, `"Ed448"`,
`"SHA256withSM2"`, `"ML-DSA"` and `"SLH-DSA"`. The concrete signers are all in `Crypto.Signers`, with
`HMacDsaKCalculator` for deterministic DSA and ECDSA.

### Elliptic curve, EdDSA and key agreement

Named curves come from four registries that share a static shape - `ECNamedCurveTable` is the union of
them all - and feed `ECKeyGenerationParameters`:

```csharp
var ecGen = new ECKeyPairGenerator("ECDSA");
ecGen.Init(new ECKeyGenerationParameters(
    ECNamedCurveTable.GetOid("secp256r1"), random));
AsymmetricCipherKeyPair ecPair = ecGen.GenerateKeyPair();
```

Ed25519 and Ed448 skip the ceremony: construct a private key parameter from a `SecureRandom`, derive
the public key from it, and sign.

```csharp
var edPrivate = new Ed25519PrivateKeyParameters(random);
var edPublic = edPrivate.GeneratePublicKey();
var edSigner = new Ed25519Signer();
edSigner.Init(forSigning: true, edPrivate);
edSigner.BlockUpdate(data, 0, data.Length);
byte[] edSignature = edSigner.GenerateSignature();
```

Agreements take *your own* private key in `Init` and the other party's public key in the calculation.
`ECDsaSigner` returns a raw `r`/`s` pair rather than a DER-encoded signature; use
`SignerUtilities.GetSigner("SHA256withECDSA")` when you want the encoded form.

```csharp
var agreement = new ECDHBasicAgreement();
agreement.Init(myPair.Private);
BigInteger z = agreement.CalculateAgreement(theirPublicKey);
byte[] shared = BigIntegers.AsUnsignedByteArray(agreement.GetFieldSize(), z);
```

Two rules go with that snippet. The fixed-width encoding is mandatory - a plain `ToByteArray()` gives
a variable-length, possibly sign-padded result that will not match the other party's. And the raw
agreement output is not a key: run it through a KDF, or use the `*WithKdf` agreement variants.

### Key encodings and PEM

`PrivateKeyFactory` reads a PKCS#8 `PrivateKeyInfo` and `PublicKeyFactory` reads a
`SubjectPublicKeyInfo`; `PrivateKeyInfoFactory` and `SubjectPublicKeyInfoFactory` are the producing
halves.

```csharp
byte[] pkcs8 = PrivateKeyInfoFactory
    .CreatePrivateKeyInfo(pair.Private).GetEncoded();
byte[] spki = SubjectPublicKeyInfoFactory
    .CreateSubjectPublicKeyInfo(pair.Public).GetEncoded();
var restoredPrivate = PrivateKeyFactory.CreateKey(pkcs8);
var restoredPublic = PublicKeyFactory.CreateKey(spki);
```

`PemReader` and `PemWriter` in `CodeBrix.Cryptography.OpenSsl` handle the text form, with
`Pkcs8Generator` for a PKCS#8 private key and an `IPasswordFinder` for an encrypted one.
`PemReader.ReadObject()` is typed `object`, and what it returns depends on the PEM label - the single
most common source of an `InvalidCastException` in this area:

```text
-----BEGIN RSA PRIVATE KEY-----          AsymmetricCipherKeyPair
-----BEGIN DSA PRIVATE KEY-----          AsymmetricCipherKeyPair
-----BEGIN EC PRIVATE KEY-----           AsymmetricCipherKeyPair
-----BEGIN PRIVATE KEY-----              AsymmetricKeyParameter  (PKCS#8)
-----BEGIN ENCRYPTED PRIVATE KEY-----    AsymmetricKeyParameter
-----BEGIN PUBLIC KEY-----               AsymmetricKeyParameter
-----BEGIN RSA PUBLIC KEY-----           AsymmetricKeyParameter (RsaKeyParameters)
-----BEGIN CERTIFICATE-----              X509Certificate
-----BEGIN X509 CERTIFICATE-----         X509Certificate
-----BEGIN X509 CRL-----                 X509Crl
-----BEGIN CERTIFICATE REQUEST-----      Pkcs10CertificationRequest
-----BEGIN NEW CERTIFICATE REQUEST-----  Pkcs10CertificationRequest
-----BEGIN ATTRIBUTE CERTIFICATE-----    X509V2AttributeCertificate
-----BEGIN EC PARAMETERS-----            Asn1.X9.X962Parameters
-----BEGIN PKCS7----- / -----BEGIN CMS-----  Asn1.Cms.ContentInfo
```

Any other label throws `IOException`, and `ReadObject()` returns null at end of stream. Handling both
private-key shapes is a switch:

```csharp
object obj = pemReader.ReadObject();
AsymmetricKeyParameter privateKey = obj switch
{
    AsymmetricCipherKeyPair kp => kp.Private,
    AsymmetricKeyParameter k when k.IsPrivate => k,
    _ => throw new InvalidOperationException("not a private key: " + obj?.GetType())
};
```

### Certificates, CRLs and key stores

`X509V3CertificateGenerator` builds a certificate - serial number, issuer and subject names, validity
window, public key and extensions - and `Generate(ISignatureFactory)` signs it. `X509V1CertificateGenerator`
and `X509V2AttributeCertificateGenerator` are the same shape for their profiles, and
`X509V2CrlGenerator` builds CRLs. Extensions are added with their OID constant and a typed value:

```csharp
certGen.AddExtension(X509Extensions.BasicConstraints, critical: true,
    new BasicConstraints(cA: false));
certGen.AddExtension(X509Extensions.KeyUsage, critical: true,
    new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
```

The signature factory lives in `CodeBrix.Cryptography.Crypto.Operators`, *not* in
`CodeBrix.Cryptography.Operators` - the latter namespace exists and holds different types.

On the reading side, `X509Certificate` exposes the fields, the validity checks and the signature
checks, and each comes in two flavors: `CheckValidity()` and `Verify()` throw, while `IsValidNow` and
`IsSignatureValid()` return a bool. `X509CertificateParser` reads DER or PEM, single objects or
bundles, and `X509CrlParser`, `X509AttrCertParser` and `X509CertPairParser` mirror it.

A PKCS#12 store is built through `Pkcs12StoreBuilder` and then filled:

```csharp
var store = new Pkcs12StoreBuilder().Build();
store.SetKeyEntry("my-key", new AsymmetricKeyEntry(pair.Private),
    new[] { new X509CertificateEntry(certificate) });
using (var fs = File.Create("store.p12"))
    store.Save(fs, "password".ToCharArray(), random);
```

### PKIX path validation

`X509Certificate.Verify` checks one signature against one key. Building and validating a chain is the
`Pkix` namespace's job, against trust anchors you supply.

```csharp
var anchors = new HashSet<TrustAnchor> { new TrustAnchor(rootCert, null) };
var target = new X509CertStoreSelector { Subject = endCert.SubjectDN };
var buildParams = new PkixBuilderParameters(anchors, target);
buildParams.AddStoreCert(CollectionUtilities.CreateStore(intermediates));
buildParams.AddStoreCrl(CollectionUtilities.CreateStore(crls));
buildParams.Date = DateTime.UtcNow;
buildParams.IsRevocationEnabled = true;
PkixCertPath path = new PkixCertPathBuilder().Build(buildParams).CertPath;
```

`PkixCertPathValidator.Validate` validates an existing path, and failures throw
`PkixCertPathBuilderException` or `PkixCertPathValidatorException`. Revocation checking is the
expensive part, so cache CRLs in the store you hand to `AddStoreCrl` rather than rebuilding it per
validation.

### CMS, OpenPGP and the message formats

CMS follows a generator-and-parser pairing throughout: `CmsSignedDataGenerator` and `CmsSignedData`,
`CmsEnvelopedDataGenerator` and `CmsEnvelopedData`, the authenticated, compressed and digested peers,
and streaming variants of each for large payloads. Digest and content-encryption algorithms are named
by constants on the generator base classes, and `SignerInformation.Verify` does the verification.

OpenPGP splits into a packet layer, `CodeBrix.Cryptography.Bcpg`, and the high-level API,
`CodeBrix.Cryptography.Bcpg.OpenPgp`: key rings, `PgpEncryptedDataGenerator`,
`PgpLiteralDataGenerator`, `PgpCompressedDataGenerator`, `PgpSignatureGenerator`, `PgpObjectFactory`
for reading, and `ArmoredOutputStream` / `ArmoredInputStream` for ASCII armor.
`PgpUtilities.GetDecoderStream` strips armor when it is present, which makes a reader work on both
forms.

OCSP, timestamping, CMP and CRMF follow the same builder idiom in their own namespaces, and ASN.1
structures throughout the library share a `GetInstance(object)` factory, with `Asn1Dump` to
pretty-print a parsed object.

### TLS and DTLS

The protocol handlers wrap a byte stream for TLS or a datagram transport for DTLS, and hand back a
`Stream` or a `DtlsTransport` once the handshake completes. The crypto is pluggable, and `BcTlsCrypto`
is the in-box implementation that needs no configuration. `AbstractTlsClient` has exactly one member
you must implement: `GetAuthentication()`.

```csharp
internal sealed class MyTlsClient : DefaultTlsClient
{
    internal MyTlsClient() : base(new BcTlsCrypto()) { }

    public override TlsAuthentication GetAuthentication() => new Authentication();

    private sealed class Authentication : TlsAuthentication
    {
        public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
        {
            // YOU must validate here. Nothing else does it for you.
            TlsCertificate[] chain =
                serverCertificate.Certificate.GetCertificateList();
            // e.g. convert each entry with
            // X509CertificateStructure.GetInstance(chain[i].GetEncoded())
            // and run a PkixCertPathBuilder / PkixCertPathValidator over it.
        }

        public TlsCredentials GetClientCredentials(
            CertificateRequest certificateRequest) => null;   // no client cert
    }
}

var tcp = new TcpClient(host, port);
var protocol = new TlsClientProtocol(tcp.GetStream());
protocol.Connect(new MyTlsClient());
Stream tls = protocol.Stream;        // read/write application data here
// ... then protocol.Close();
```

> [!WARNING]
> `NotifyServerCertificate` is where certificate validation happens, and the default implementation
> does nothing. A client that leaves it empty will happily talk to any server presenting any
> certificate.

Override `GetProtocolVersions()` to pin the protocol range and `GetSupportedCipherSuites()` to choose
suites; alerts arrive through `NotifyAlertRaised` and `NotifyAlertReceived`.

### Post-quantum and KEM

ML-KEM, ML-DSA and SLH-DSA live in the main `Crypto` namespaces rather than under `Pqc`. Parameter
sets are static instances - `MLKemParameters.ml_kem_512`, `ml_kem_768` and `ml_kem_1024`, with
`MLDsaParameters` and `SlhDsaParameters` following the same shape - and each has a key pair generator.
Encapsulation is `IKemEncapsulator` in `Crypto.Kems`, with `MLKemDecapsulator` as its peer. The
remaining NIST candidates live under `CodeBrix.Cryptography.Pqc.Crypto.*`, each with its own
parameters, generators and signer or KEM types. Re-read the experimental warning above before using
any of them.

### Runtime configuration

A number of hardening limits - ASN.1 parse depth, Argon2 and PBE cost ceilings, key-size ceilings and
similar - are configurable at run time through `CodeBrix.Cryptography.Utilities.Properties`. Each is
looked up first in the thread-local property table and then in the process environment; the API reads
environment variables but never modifies them.

```csharp
Properties.SetThreadInt32(Properties.Asn1MaxDepth, 64);
```

The keys are exposed as `public static readonly string` fields on `Properties` - `Asn1MaxDepth`,
`Asn1MaxLimit`, `Asn1AllowUnsafeInteger`, `RsaMaxSize`, `RsaMaxMRTests`, `RsaAllowUnsafeModulus`,
`DsaMaxSize`, `DHMaxSize`, `ECFpMaxSize`, `ECF2mMaxSize`, `ECFpCertainty`, `Pkcs12MaxIterationCount`,
`Pkcs12IgnoreUselessPassword`, `PbeMaxIterationCount`, `PbeMaxScryptMemory`, `Argon2MaxMemoryExp`,
`Argon2MaxPasses`, `Argon2MaxParallelism`, `Pkcs1NotStrict`, `PKMacMaxIterationCount`,
`CmsAllowLenientRsaPkcs1`, `FpeDisable`, `FpeDisableFf1`, `X509MaxPolicyNodes`,
`X509AllowNonDerTbsCertificate`, `X509AllowLenientRfc822Name`, `X509AllowEmptyIssuerCert`,
`X509Sgp22NameConstraints` and `X509AllowLenientIPAddressMask`. Reference the field rather than
retyping its literal, and the spelling can never drift. Change these only to raise or lower the
defaults deliberately; the shipped values are the safe ones.

### The error model

There is no single exception base - each layer has its own.

```text
CodeBrix.Cryptography.Crypto
    CryptoException                base for algorithm-layer failures
    InvalidCipherTextException     AEAD tag mismatch, bad padding, bad
                                   RSA block - treat as "authentication or
                                   decryption failed", never ignore
    DataLengthException            input/output buffer wrong size
    OutputLengthException          output buffer too small
    MaxBytesExceededException      stream cipher used past its limit
CodeBrix.Cryptography.Asn1
    Asn1Exception, Asn1ParsingException
CodeBrix.Cryptography.Security
    GeneralSecurityException       base for the facade
    SecurityUtilityException       unknown algorithm name or OID
    InvalidKeyException, InvalidParameterException, KeyException,
    SignatureException, EncryptionException, PasswordException
CodeBrix.Cryptography.Security.Certificates
    CertificateException, CertificateEncodingException,
    CertificateParsingException, CertificateExpiredException,
    CertificateNotYetValidException, CrlException
CodeBrix.Cryptography.Cms          CmsException, CmsStreamException,
                                   CmsVerifierCertificateNotValidException
CodeBrix.Cryptography.Pkcs         PkcsException, PkcsIOException
CodeBrix.Cryptography.Bcpg.OpenPgp PgpException, PgpDataValidationException,
                                   PgpKeyValidationException
CodeBrix.Cryptography.Tls          TlsFatalAlert, TlsTimeoutException,
                                   TlsNoCloseNotifyException
CodeBrix.Cryptography.Ocsp         OcspException
CodeBrix.Cryptography.Tsp          TspException, TspValidationException
CodeBrix.Cryptography.Pkix         PkixCertPathBuilderException,
                                   PkixCertPathValidatorException,
                                   PkixNameConstraintValidatorException
CodeBrix.Cryptography.Cmp          CmpException
CodeBrix.Cryptography.Crmf         CrmfException
CodeBrix.Cryptography.OpenSsl      PemException, PasswordException,
                                   EncryptionException
```

The ordinary `ArgumentException`, `ArgumentNullException`, `InvalidOperationException`, `IOException`
and `EndOfStreamException` cover misuse and truncated input.

## Examples

AES-256-GCM end to end, including the authentication-failure path that most samples leave out.

```csharp
using System;
using System.Text;
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Modes;
using CodeBrix.Cryptography.Crypto.Parameters;
using CodeBrix.Cryptography.Security;

var random = new SecureRandom();

byte[] key = new byte[32];        // AES-256
byte[] nonce = new byte[12];      // 96-bit nonce; NEVER reuse with one key
random.NextBytes(key);
random.NextBytes(nonce);

byte[] plaintext = Encoding.UTF8.GetBytes("attack at dawn");
byte[] associatedData = Encoding.UTF8.GetBytes("message-id: 42");

// Encrypt - the 128-bit tag is appended to the ciphertext.
var encryptor = new GcmBlockCipher(AesUtilities.CreateEngine());
encryptor.Init(forEncryption: true,
    new AeadParameters(new KeyParameter(key), 128, nonce, associatedData));

byte[] ciphertext = new byte[encryptor.GetOutputSize(plaintext.Length)];
int written = encryptor.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
written += encryptor.DoFinal(ciphertext, written);

// Decrypt - DoFinal throws InvalidCipherTextException if the tag is wrong.
var decryptor = new GcmBlockCipher(AesUtilities.CreateEngine());
decryptor.Init(forEncryption: false,
    new AeadParameters(new KeyParameter(key), 128, nonce, associatedData));

byte[] recovered = new byte[decryptor.GetOutputSize(written)];
int n = decryptor.ProcessBytes(ciphertext, 0, written, recovered, 0);
try
{
    n += decryptor.DoFinal(recovered, n);
    Console.WriteLine(Encoding.UTF8.GetString(recovered, 0, n));  // attack at dawn
}
catch (InvalidCipherTextException)
{
    Console.WriteLine("authentication failed - do not use the plaintext");
}
```

An RSA key pair, a signature, and the standard key encodings round-tripped, which is what a "sign this
and let someone else verify it" task actually needs.

```csharp
using System;
using System.Text;
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Pkcs;
using CodeBrix.Cryptography.Security;
using CodeBrix.Cryptography.X509;

var random = new SecureRandom();

var generator = new RsaKeyPairGenerator();
generator.Init(new KeyGenerationParameters(random, 3072));
AsymmetricCipherKeyPair pair = generator.GenerateKeyPair();

byte[] data = Encoding.UTF8.GetBytes("the message to sign");

ISigner signer = SignerUtilities.InitSigner(
    "SHA256withRSA", forSigning: true, pair.Private, random);
signer.BlockUpdate(data, 0, data.Length);
byte[] signature = signer.GenerateSignature();

ISigner verifier = SignerUtilities.InitSigner(
    "SHA256withRSA", forSigning: false, pair.Public, null);
verifier.BlockUpdate(data, 0, data.Length);
Console.WriteLine(verifier.VerifySignature(signature));      // True

// Round-trip the keys through the standard encodings.
byte[] pkcs8 = PrivateKeyInfoFactory
    .CreatePrivateKeyInfo(pair.Private).GetEncoded();
byte[] spki = SubjectPublicKeyInfoFactory
    .CreateSubjectPublicKeyInfo(pair.Public).GetEncoded();

var reloadedPublic = PublicKeyFactory.CreateKey(spki);
ISigner verifier2 = SignerUtilities.InitSigner(
    "SHA256withRSA", forSigning: false, reloadedPublic, null);
verifier2.BlockUpdate(data, 0, data.Length);
Console.WriteLine(verifier2.VerifySignature(signature));      // True
Console.WriteLine(pkcs8.Length > 0);                          // True
```

A self-signed certificate with the extensions a real one carries, written as PEM and read back.

```csharp
using System;
using System.IO;
using CodeBrix.Cryptography.Asn1.X509;
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Crypto.Operators;
using CodeBrix.Cryptography.Math;
using CodeBrix.Cryptography.OpenSsl;
using CodeBrix.Cryptography.Security;
using CodeBrix.Cryptography.X509;

var random = new SecureRandom();

var keyPairGenerator = new RsaKeyPairGenerator();
keyPairGenerator.Init(new KeyGenerationParameters(random, 3072));
AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();

var name = new X509Name("CN=example.test, O=Acme, C=US");

var certificateGenerator = new X509V3CertificateGenerator();
certificateGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, random));
certificateGenerator.SetIssuerDN(name);
certificateGenerator.SetSubjectDN(name);
certificateGenerator.SetNotBefore(DateTime.UtcNow.AddMinutes(-5));
certificateGenerator.SetNotAfter(DateTime.UtcNow.AddYears(1));
certificateGenerator.SetPublicKey(keyPair.Public);

certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true,
    new BasicConstraints(cA: false));
certificateGenerator.AddExtension(X509Extensions.KeyUsage, true,
    new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false,
    new ExtendedKeyUsage(KeyPurposeID.id_kp_serverAuth));
certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false,
    new GeneralNames(new GeneralName(GeneralName.DnsName, "example.test")));

var signatureFactory = new Asn1SignatureFactory(
    "SHA256WITHRSA", keyPair.Private, random);
X509Certificate certificate = certificateGenerator.Generate(signatureFactory);

certificate.CheckValidity();                    // throws if outside validity
certificate.Verify(keyPair.Public);             // throws on a bad signature

Console.WriteLine(certificate.SubjectDN);
Console.WriteLine(certificate.SigAlgName);
Console.WriteLine(certificate.SerialNumber);

// Persist as PEM, then read it back.
var certWriter = new StringWriter();
using (var pemWriter = new PemWriter(certWriter))
{
    pemWriter.WriteObject(certificate);                        // "CERTIFICATE"
}
File.WriteAllText("cert.pem", certWriter.ToString());

var parsed = new X509CertificateParser()
    .ReadCertificate(File.ReadAllBytes("cert.pem"));
Console.WriteLine(parsed.SubjectDN.Equivalent(certificate.SubjectDN));   // True
```

<details>
<summary>Signing that document with CMS, then verifying it</summary>

```csharp
using System;
using System.Linq;
using System.Text;
using CodeBrix.Cryptography.Cms;
using CodeBrix.Cryptography.Utilities.Collections;
using CodeBrix.Cryptography.X509;

// 'keyPair' and 'certificate' come from the certificate example above.
byte[] data = Encoding.UTF8.GetBytes("the document to sign");

var generator = new CmsSignedDataGenerator();
generator.AddSigner(keyPair.Private, certificate, CmsSignedGenerator.DigestSha256);
generator.AddCertificates(CollectionUtilities.CreateStore(new[] { certificate }));

CmsSignedData signed = generator.Generate(
    new CmsProcessableByteArray(data), encapsulate: true);
byte[] encoded = signed.GetEncoded();

// Verify.
var reloaded = new CmsSignedData(encoded);
var certStore = reloaded.GetCertificates();

foreach (SignerInformation signer in reloaded.GetSignerInfos().GetSigners())
{
    X509Certificate signerCert = certStore
        .EnumerateMatches(signer.SignerID)
        .First();
    Console.WriteLine(signer.Verify(signerCert));       // True
}
```

For a detached signature, pass `encapsulate: false` and reconstruct with
`new CmsSignedData(new CmsProcessableByteArray(data), encoded)`.

</details>

## Using it in a CodeBrix.Platform application

There is nothing head-specific to arrange. The library is plain managed code with no native assets and
no P/Invoke, so one build works on every head and on every operating system; the x86 intrinsic paths
are probed once at run time and fall back to portable managed code where they are unavailable. Put the
package reference in the library that needs it - usually your `.Core` project - and use it directly.
There is no registration call.

## Pitfalls

- `InvalidCipherTextException` from an AEAD `DoFinal` *is* the authentication failure. Never
  catch-and-continue with the partially written output buffer; treat the whole message as forged and
  discard the plaintext.
- Never reuse a key and nonce pair with GCM, CCM, EAX, OCB or ChaCha20-Poly1305. Nonce reuse with a
  counter-mode AEAD leaks the authentication key, not only the plaintext. Generate the nonce randomly
  per message, or use a strictly increasing counter you persist. `GcmSivBlockCipher` is the
  nonce-misuse-resistant option when you cannot guarantee uniqueness.
- `AeadParameters` takes `macSize` in **bits** - 128, not 16 - and
  `PbeParametersGenerator.GenerateDerivedParameters` takes key and IV sizes in bits too. Passing bytes
  silently produces a much weaker result.
- Key-format confusion is the most common integration bug here. `PrivateKeyFactory.CreateKey` expects a
  PKCS#8 encoding and `PublicKeyFactory.CreateKey` expects a `SubjectPublicKeyInfo`; feeding one to the
  other throws, but the error rarely says "wrong format". `AsymmetricKeyParameter.IsPrivate` is the
  cheap way to tell which half of a pair you are holding, because `Init(forSigning: true, publicKey)`
  fails late.
- `PemReader.ReadObject()` returns an `AsymmetricCipherKeyPair` for the legacy private-key labels and
  an `AsymmetricKeyParameter` for the PKCS#8 label. Handle both.
- `Asn1SignatureFactory` and `Asn1VerifierFactory` live in `CodeBrix.Cryptography.Crypto.Operators`,
  not `CodeBrix.Cryptography.Operators`. The latter namespace exists and holds different types.
- Raw RSA has no padding at all and cannot encrypt more than `GetInputBlockSize()` bytes. Always wrap
  it - `OaepEncoding` for encryption - and prefer `RSABlindedEngine` for private-key operations.
- An ECDH or X25519 agreement result is not a key. Run it through a KDF, and convert the `BigInteger`
  with `BigIntegers.AsUnsignedByteArray(fieldSize, z)` rather than `ToByteArray()`.
- TLS certificate validation is your job. An empty `NotifyServerCertificate` accepts every
  certificate; wire it to the path builder and validator or an equivalent check.
- `X509Certificate.Verify(publicKey)` only checks the signature against the key you hand it. It does
  not check validity dates, revocation, name matching or chain building - that is what the `Pkix`
  namespace is for.
- Post-quantum algorithms are experimental. Their APIs, parameter sets and encodings can change
  between releases, so do not persist data or keys that have to survive an upgrade.
- This library's `Math.BigInteger` is not `System.Numerics.BigInteger`, and its
  `Utilities.Collections` interfaces are not the `System.Collections` ones. A file that needs both
  worlds should alias one of them.
- Cipher, digest, MAC, signer and generator objects are stateful and not thread-safe. `SecureRandom` is
  safe to share; almost nothing else is. Reuse initialized instances and call `Reset()` between
  operations, one per thread or from a pool.
- Comparing secrets with a sequence equality helper or a plain loop leaks timing. Use
  `CodeBrix.Cryptography.Utilities.Arrays.FixedTimeEquals`, or the AEAD and MAC verification the
  library already provides.
- Do not use MD5, SHA-1, DES, RC4 or 1024-bit RSA in new work. They are present because
  interoperability and test vectors demand them, not as recommendations.
- Some members are marked obsolete - older CMS generators, legacy signature entry points, a few
  constructors. The compiler warning is the guidance; switch to the replacement it names rather than
  suppressing it.
- `PemWriter` and `PemReader` wrap a `TextWriter` and a `TextReader`. Flush or dispose the writer
  before reading the underlying buffer back, or you will get a truncated PEM block.
- RSA key generation at 3072 bits and above takes real time. Do it once, off the request path, and
  persist the key.
- Prefer the span-based overloads on hot paths, size output buffers once with `GetOutputSize()` and
  write into a single buffer, and use the streaming wrappers - `CipherStream`, `DigestStream`,
  `MacStream`, `SignerStream` and the CMS and OpenPGP stream generators - for large payloads.

## Samples and tools in the repository

The repository ships no sample applications, demo applications or tools. The only content outside the
packaged library is the test project and its fixture data - and for a consumer, that suite is the
richest set of worked examples available, with a feature-to-test-file map in AGENT-README.txt.

| Name | What it demonstrates | Where |
| --- | --- | --- |
| Test suite | Every feature area, in a folder tree that mirrors the library's namespaces | [`tests/CodeBrix.Cryptography.Tests`](https://github.com/ellisnet/CodeBrix.Cryptography/tree/main/tests/CodeBrix.Cryptography.Tests) |
| Fixture data | Keys, certificates, CRLs and message samples, embedded in the test assembly so they never depend on the working directory | [`tests/CodeBrix.Cryptography.Tests/data`](https://github.com/ellisnet/CodeBrix.Cryptography/tree/main/tests/CodeBrix.Cryptography.Tests/data) |
| Fixture generators | The generators that produce the self-authored half of the fixtures; they are skipped unless `CODEBRIX_CRYPTOGRAPHY_REGENERATE_TEST_DATA=1` is set, so they never run in a normal pass | [`tests/CodeBrix.Cryptography.Tests/TestDataGeneration`](https://github.com/ellisnet/CodeBrix.Cryptography/tree/main/tests/CodeBrix.Cryptography.Tests/TestDataGeneration) |
| Repository fixtures | Credentials, certificates, CMS samples, ASN.1 stress files and the public-domain interoperability archives the suite reads | [`test-data`](https://github.com/ellisnet/CodeBrix.Cryptography/tree/main/test-data) |

Where to look for a worked example of the thing you are writing: `Crypto/Tests/` for ciphers, modes,
digests and MACs, `Crypto/Tests/Argon2Test.cs`, `BCryptTest.cs`, `SCryptTest.cs` and
`HkdfGeneratorTest.cs` for key derivation, `Tests/RSATest.cs` and `Tests/ECDSA5Test.cs` for signatures,
`OpenSsl/Tests/` for PEM, `Tests/CertTest.cs` and `X509/Tests/TestCertificateGen.cs` for certificates,
`Tests/CertPathBuilderTest.cs` and `Tests/CertPathValidatorTest.cs` for PKIX, `Cms/Tests/` for CMS,
`Bcpg/OpenPgp/Tests/` for OpenPGP, and `Tls/Tests/` for TLS and DTLS - where the mock client and server
peers show a complete implementation of both sides.

A subset of the suite is skipped unless an external, non-redistributable vector set is present. Those
tests are still readable as examples.

## Documentation and source

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/AGENT-README.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/EXTRAS-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/README-INDEX.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.Cryptography.Tests](https://github.com/ellisnet/CodeBrix.Cryptography/tree/main/tests/CodeBrix.Cryptography.Tests) |

XML documentation ships alongside the assembly, so every type and member described here is available
to IntelliSense.

## License

CodeBrix.Cryptography is licensed under the MIT License, and the license is also named in the package
ID (`CodeBrix.Cryptography.MitLicenseForever`). For the provenance and licensing of open source code
included in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Cryptography/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.SSH](CodeBrix.SSH.md) - an SSH-2 client built on this library, for keys, shells and file transfer
- [CodeBrix.Sqlite](CodeBrix.Sqlite.md) - encrypted columns and a searchable blind index, with the cipher work already done
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.Cryptography on GitHub](https://github.com/ellisnet/CodeBrix.Cryptography) - source and tests
