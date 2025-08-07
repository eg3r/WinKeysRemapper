# 🚀 **How to Create Releases**

With GitHub Actions handling everything, creating releases is simple:

## **Method 1: Command Line**
```bash
git tag v1.0.0
git push origin v1.0.0
```

## **Method 2: GitHub UI** (Recommended)
1. Go to your repository on GitHub
2. Click **"Releases"** → **"Create a new release"**
3. Click **"Choose a tag"** → Type `v1.0.0` → **"Create new tag"**
4. Add release title and notes
5. Click **"Publish release"**

## **What Happens Automatically:**
- ✅ Builds x64, x86, and portable versions
- ✅ Creates optimized ZIP files
- ✅ Uploads to GitHub Releases
- ✅ Proper version numbering

**That's it!** No scripts needed - GitHub Actions does all the heavy lifting.
