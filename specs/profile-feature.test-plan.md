# User Profile Feature Test Plan

## Application Overview

This test plan covers the User Profile feature in Talk With Tiles application. The profile page is accessed through the user dropdown menu and displays basic user information including Name, Email, and User ID for authenticated users. Tests focus on data accuracy, authentication requirements, navigation, and proper display of user information.

## Test Scenarios

### 1. User Profile Access and Authentication

**Seed:** `tests/seed.spec.ts`

#### 1.1. Authenticated User Can Access Profile

**File:** `tests/profile/profile-access.spec.ts`

**Steps:**
  1. Navigate to homepage with authenticated user
    - expect: User should be logged in
    - expect: Navigation should show user's name in dropdown
  2. Click on user dropdown menu (user's name)
    - expect: Dropdown menu should expand
    - expect: Profile option should be visible
    - expect: Profile link should be present
  3. Click on 'Profile' link from dropdown menu
    - expect: Should navigate to /profile URL
    - expect: Page title should be 'Profile - Talk With Tiles'
    - expect: Profile page should load successfully

#### 1.2. Unauthenticated User Cannot Access Profile

**File:** `tests/profile/profile-unauthorized.spec.ts`

**Steps:**
  1. Clear authentication and navigate directly to /profile
    - expect: Should redirect to login page or show authentication error
    - expect: Profile content should not be accessible
  2. Verify navigation menu for anonymous user
    - expect: User dropdown should not be present
    - expect: Profile option should not be available in navigation

### 2. Profile Information Display and Validation

**Seed:** `tests/seed.spec.ts`

#### 2.1. Profile Shows Correct User Information

**File:** `tests/profile/profile-information.spec.ts`

**Steps:**
  1. Navigate to profile page as authenticated user
    - expect: Profile page should load
    - expect: Page should show 'Your Profile' heading
  2. Verify Name field is displayed
    - expect: Name label should be present
    - expect: User's actual name should be displayed
    - expect: Name should match the logged-in user (e.g., 'Alice')
  3. Verify Email field is displayed
    - expect: Email label should be present
    - expect: User's email address should be displayed
    - expect: Email should be in valid format (e.g., alice@test.local)
  4. Verify User ID field is displayed
    - expect: User ID label should be present
    - expect: User ID should be displayed
    - expect: User ID should be in GUID format (e.g., 11111111-1111-1111-1111-111111111111)

#### 2.2. Profile Information Consistency

**File:** `tests/profile/profile-consistency.spec.ts`

**Steps:**
  1. Note the user name displayed in navigation dropdown
    - expect: User name should be visible in navigation
  2. Navigate to profile page and compare name
    - expect: Name in profile should exactly match name in navigation
    - expect: No discrepancies in user identification
  3. Verify email consistency across the application
    - expect: Email shown in profile should match what's used for authentication
    - expect: Email format should be consistent

### 3. Profile Page Layout and UI Validation

**Seed:** `tests/seed.spec.ts`

#### 3.1. Profile Page Layout and Structure

**File:** `tests/profile/profile-layout.spec.ts`

**Steps:**
  1. Navigate to profile page
    - expect: Page should load without errors
    - expect: Standard page layout with header and footer should be present
  2. Verify page heading and structure
    - expect: 'Your Profile' heading should be displayed prominently
    - expect: Information should be organized in a definition list format
    - expect: Labels and values should be properly aligned
  3. Check the avatar or initial display
    - expect: User avatar or initial (e.g., 'A' for Alice) should be displayed
    - expect: Avatar should be appropriately sized and positioned
  4. Verify all profile fields are non-editable
    - expect: Name field should be read-only
    - expect: Email field should be read-only
    - expect: User ID field should be read-only
    - expect: No edit functionality should be available

#### 3.2. Profile Page Responsive Design

**File:** `tests/profile/profile-responsive.spec.ts`

**Steps:**
  1. Test profile page on mobile viewport (375px width)
    - expect: Page should be responsive
    - expect: All information should be readable
    - expect: Layout should adapt appropriately
  2. Test profile page on tablet viewport (768px width)
    - expect: Page should display correctly
    - expect: Information should be well-organized
    - expect: No horizontal scrolling required
  3. Test profile page on desktop viewport (1200px width)
    - expect: Page should utilize space effectively
    - expect: Layout should be visually balanced
    - expect: All elements should be properly sized

### 4. Profile Navigation and User Experience

**Seed:** `tests/seed.spec.ts`

#### 4.1. Profile Navigation Flow

**File:** `tests/profile/profile-navigation.spec.ts`

**Steps:**
  1. Start from homepage and navigate to profile
    - expect: Navigation should be smooth
    - expect: Breadcrumb or back navigation should be available
  2. From profile page, test navigation to other sections
    - expect: Header navigation should remain functional
    - expect: Can navigate to Home, Scrabble, About, Feedback pages
    - expect: Navigation state should be preserved
  3. Test browser back button functionality
    - expect: Back button should work correctly
    - expect: Should return to previous page
    - expect: Page state should be maintained
  4. Test direct URL access to profile
    - expect: Direct navigation to /profile should work
    - expect: Page should load completely
    - expect: User should remain authenticated

#### 4.2. Profile Page Performance

**File:** `tests/profile/profile-performance.spec.ts`

**Steps:**
  1. Measure profile page load time
    - expect: Page should load within 2 seconds
    - expect: No JavaScript errors should occur
    - expect: All content should be visible immediately
  2. Test multiple profile page visits
    - expect: Subsequent visits should be faster due to caching
    - expect: No memory leaks should occur
    - expect: Page should consistently load correctly

### 5. Profile Security and Edge Cases

**Seed:** `tests/seed.spec.ts`

#### 5.1. Profile Data Security

**File:** `tests/profile/profile-security.spec.ts`

**Steps:**
  1. Inspect page source and network requests
    - expect: No sensitive information should be exposed in HTML
    - expect: User ID should not reveal internal system details
    - expect: No additional user data should be leaked
  2. Test profile access after session timeout
    - expect: User should be redirected to login if session expires
    - expect: Profile data should not be cached inappropriately
    - expect: Proper authentication flow should be enforced

#### 5.2. Profile Edge Cases and Data Validation

**File:** `tests/profile/profile-edge-cases.spec.ts`

**Steps:**
  1. Test profile with user having special characters in name
    - expect: Special characters should be displayed correctly
    - expect: No encoding issues should occur
    - expect: Layout should not break
  2. Test profile with very long email addresses
    - expect: Long emails should be displayed properly
    - expect: Text wrapping should work correctly
    - expect: No overflow issues should occur
  3. Test profile display consistency across different browsers
    - expect: Information should appear identically
    - expect: Layout should be consistent
    - expect: No browser-specific rendering issues
